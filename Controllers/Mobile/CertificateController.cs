using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class CertificateController : Controller
    {

        public CertificateController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> read([FromBody] Criteria value)
        {
            try
            {

                //value.statisticsCreate("certificate");
                var col = new Database().MongoClient<Certificate>("certificate");
                var filter = Builders<Certificate>.Filter.Eq("status", "A");

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Certificate>.Filter.Regex("code", value.code); }
           
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<Certificate>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("CertificateKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Certificate>.Filter.Eq("category", value.category); }

                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Certificate>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<Certificate>.Filter.Eq("isHighlight", value.isHighlight); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Certificate>.Filter.Gt("docDate", ds.start) & Builders<Certificate>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Certificate>.Filter.Gt("docDate", ds.start) & Builders<Certificate>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Certificate>.Filter.Gt("docDate", de.start) & Builders<Certificate>.Filter.Lt("docDate", de.end); }

                List<Certificate> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("certificateCategory", "category", "code", "categoryList")
                                      .As<Certificate>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        new StatisticService(value.code, docs[0].title, "certificate", value.profileCode, "", "");

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";

                        value.statisticsCreateAsync("certificate");
                    }
                }
                catch { }
                //END : Statistic

                //BEGIN : Statistic category
                try
                {
                    var valueCategory = new Criteria();
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        valueCategory.reference = docs.Count > 0 ? docs[0].categoryList[0].code : "";
                        valueCategory.title = docs.Count > 0 ? docs[0].categoryList[0].title : "";
                    }

                    valueCategory.statisticsCreateAsync("certificateCategory");
                }
                catch { }
                //END : Statistic

                var result = new List<object>();
                docs.ForEach(c =>
                {
                    try
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var colCenter = new Database().MongoClient<Register>("mCenter");
                        var filterRegister = Builders<Register>.Filter.Eq("username", c.updateBy);
                        Register docRegister = colRegister.Find(filterRegister).Project(c => new Register { imageUrl = c.imageUrl, firstName = c.firstName, lastName = c.lastName, center = c.center }).FirstOrDefault();

                        c.imageUrlCreateBy = docRegister.imageUrl;
                        c.updateBy = docRegister.firstName + " " + docRegister.lastName;

                        var filterRegister2 = Builders<Register>.Filter.Eq("username", c.createBy);
                        Register docRegister2 = colRegister.Find(filterRegister2).Project(c => new Register { imageUrl = c.imageUrl, firstName = c.firstName, lastName = c.lastName, center = c.center }).FirstOrDefault();
                        c.createBy = docRegister2.firstName + " " + docRegister2.lastName;

                        result.Add(new
                        {
                            code = c.code,
                            title = c.title,
                            titleEN = c.titleEN != "" ? c.titleEN : c.title,
                            certified = c.certified,
                            imageUrl = c.imageUrl,
                            sequence = c.sequence,
                            category = c.category,
                            description = c.description,
                            descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                            c.fileUrl,
                            createBy = c.createBy,
                            createDate = c.createDate,
                            createTime = c.createTime,
                            updateBy = c.updateBy,
                            updateDate = c.updateDate,
                            updateTime = c.updateTime,
                            isActive = c.isActive,
                            status = c.status,
                            docDate = c.docDate,
                            docTime = c.docTime,

                        });
                    }
                    catch
                    {

                    }
                });

                return new Response { status = "S", message = "success", objectData = result, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("readAll")]
        public ActionResult<Response> readAll([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Certificate>("certificate");
                var filter = Builders<Certificate>.Filter.Eq("status", "A");

                List<Certificate> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).ToList();

                //Get Category
                var colCategory = new Database().MongoClient<Category>("certificateCategory");
                var filterCategory = Builders<Category>.Filter.Eq("isActive", true);
                List<Category> docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code,title = c.title, sequence = c.sequence }).ToList();

                var result = docs.GroupBy(g => g.category).Select(s => new {
                    code = s.Key,
                    sequence = docCategory.Where(x => x.code == s.Key).FirstOrDefault().sequence,
                    title = docCategory.Where(x => x.code == s.Key).FirstOrDefault().title,
                    data = s.Select(c => new {
                        code = c.code,
                        title = c.title,
                        titleEN = c.titleEN,
                        certified = c.certified,
                        imageUrl = c.imageUrl,
                        c.fileUrl,
                        sequence = c.sequence,
                        category = c.category,
                    }).OrderBy(o => o.sequence).ToList()
                }).ToList();

                result = result.OrderBy(c => c.sequence).ToList();

                return new Response { status = "S", message = "success", objectData = result, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("certificateCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<certificateCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<certificateCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                ////BEGIN : Statistic
                //try
                //{
                //    if (!string.IsNullOrEmpty(value.code))
                //    {
                //        value.reference = value.code;
                //        value.title = docs.Count > 0 ? docs[0].title : "";
                //    }

                //    value.statisticsCreateAsync("newCategory");
                //}
                //catch { }
                ////END : Statistic

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("CertificateGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("galleryFile/read")]
        public ActionResult<Response> GalleryFileRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("CertificateGalleryFile");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.title).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code, c.type, c.title, c.size }).ToList();

                docs = docs.OrderBy(c => c.title.PadNumbers()).ToList();
                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}