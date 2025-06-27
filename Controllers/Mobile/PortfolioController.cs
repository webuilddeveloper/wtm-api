using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cms_api.Controllers.Mobile
{
    [Route("m/[controller]")]
    public class PortfolioController : Controller
    {
        public PortfolioController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> readMain([FromBody] Criteria value)
        {
            try
            {

                //value.statisticsCreate("product");
                var col = new Database().MongoClient<Portfolio>("portfolio");
                var filter = Builders<Portfolio>.Filter.Eq("status", "A");

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Portfolio>.Filter.Regex("code", value.code); }

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<Portfolio>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("PortfolioKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }
                if (!string.IsNullOrEmpty(value.status2)) { filter = filter & Builders<Portfolio>.Filter.Eq("status2", value.status2); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Portfolio>.Filter.Eq("category", value.category); }

                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Portfolio>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<Portfolio>.Filter.Eq("isHighlight", value.isHighlight); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Portfolio>.Filter.Gt("docDate", ds.start) & Builders<Portfolio>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Portfolio>.Filter.Gt("docDate", ds.start) & Builders<Portfolio>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Portfolio>.Filter.Gt("docDate", de.start) & Builders<Portfolio>.Filter.Lt("docDate", de.end); }

                List<Portfolio> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("portfolioCategory", "category", "code", "categoryList")
                                      .As<Portfolio>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        new StatisticService(value.code, docs[0].title, "portfolio", value.profileCode, "", "");

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";

                        value.statisticsCreateAsync("portfolio");
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

                    valueCategory.statisticsCreateAsync("portfolioCategory");
                }
                catch { }
                //END : Statistic

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("portfolio");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                          .Lookup("portfolioCategory", "category", "code", "categoryList")
                                          .As<Portfolio>()
                                          .ToList();

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
                            imageBanner = c.imageBanner,
                            imageExample = c.imageExample,
                            sequence = c.sequence,
                            category = c.category,
                            description = c.description,
                            descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                            createBy = c.createBy,
                            createDate = c.createDate,
                            createTime = c.createTime,
                            updateBy = c.updateBy,
                            updateDate = c.updateDate,
                            updateTime = c.updateTime,
                            isActive = c.isActive,
                            status = c.status,
                            functionList = c.functionList,
                            //isHighlight = c.isHighlight,
                            //isPublic = c.isPublic,
                            //isNotification = c.isNotification,
                            docDate = c.docDate,
                            docTime = c.docTime,
                            textButton = c.textButton,
                            linkUrl = c.linkUrl,
                            //fileUrl = c.fileUrl,
                            status2 = c.status2,
                            progress = c.progress,

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
                var col = new Database().MongoClient<Gallery>("PortfolioGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(x => x.sequence).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code, c.sequence }).ToList();

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
                var col = new Database().MongoClient<Gallery>("PortfolioGalleryFile");

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

        // POST /read
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<Comment>("PortfolioComment");

                var filter = Builders<Comment>.Filter.Ne("status", "D") & Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                List<Comment> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit).ToList();

                docs.ForEach(c =>
                {
                    if (!string.IsNullOrEmpty(c.profileCode))
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", c.profileCode);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                        c.imageUrlCreateBy = docRegister.imageUrl;
                    }
                });

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("comment/create")]
        public ActionResult<Response> CommentCreate([FromBody] Comment value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {

                //Get Profile
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode);
                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                var word = value.description.verifyRude();

                var col = new Database().MongoClient("PortfolioComment");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "description", word },
                    { "original", value.description },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "profileCode", value.profileCode },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "reference", value.reference }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("portfolioCategory");

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
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<productCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<productCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.titleEN, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

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
        [HttpPost("count")]
        public ActionResult<Response> Count([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Portfolio>("portfolio");
                var filter = (Builders<Portfolio>.Filter.Eq("status", "A") & value.filterOrganization<Portfolio>());
                var docs = col.CountDocuments(filter);

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}

