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
    public class WorkProcessController : Controller
    {

        public WorkProcessController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> readMain([FromBody] Criteria value)
        {
            try
            {

                //value.statisticsCreate("workProcess");
                var col = new Database().MongoClient<WorkProcess>("workProcess");
                var filter = Builders<WorkProcess>.Filter.Eq("status", "A");

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<WorkProcess>.Filter.Regex("code", value.code); }
           
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<WorkProcess>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("WorkProcessKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<WorkProcess>.Filter.Eq("category", value.category); }

                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<WorkProcess>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<WorkProcess>.Filter.Eq("isHighlight", value.isHighlight); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<WorkProcess>.Filter.Gt("docDate", ds.start) & Builders<WorkProcess>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<WorkProcess>.Filter.Gt("docDate", ds.start) & Builders<WorkProcess>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<WorkProcess>.Filter.Gt("docDate", de.start) & Builders<WorkProcess>.Filter.Lt("docDate", de.end); }

                List<WorkProcess> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("WorkProcessCategory", "category", "code", "categoryList")
                                      .As<WorkProcess>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        new StatisticService(value.code, docs[0].title, "workProcess", value.profileCode, "", "");

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";

                        value.statisticsCreateAsync("workProcess");
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

                    valueCategory.statisticsCreateAsync("WorkProcessCategory");
                }
                catch { }
                //END : Statistic

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("workProcess");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                          .Lookup("WorkProcessCategory", "category", "code", "categoryList")
                                          .As<WorkProcess>()
                                          .ToList();

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
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
                            imageUrl = c.imageUrl,
                            //sequence = c.sequence,
                            //category = c.category,
                            description = c.description,
                            descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                            //createBy = c.createBy,
                            //createDate = c.createDate,
                            //createTime = c.createTime,
                            //updateBy = c.updateBy,
                            //updateDate = c.updateDate,
                            //updateTime = c.updateTime,
                            //isActive = c.isActive,
                            //status = c.status,
                            //isHighlight = c.isHighlight,
                            //isPublic = c.isPublic,
                            //isNotification = c.isNotification,
                            //docDate = c.docDate,
                            //docTime = c.docTime,
                            //center = colCenter.Find(o => o.code == c.center).FirstOrDefault()?.title ?? "",
                            //textButton = c.textButton,
                            //linkUrl = c.linkUrl,
                            //fileUrl = c.fileUrl,

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
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("WorkProcessGallery");

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
                var col = new Database().MongoClient<Gallery>("WorkProcessGalleryFile");

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