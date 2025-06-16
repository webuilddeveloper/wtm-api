using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace external_api.Controllers
{
    [Route("ext/[controller]")]
    public class NewsController : Controller
    {

        public NewsController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var aesService = new AES();
                var decryptData = aesService.AesDecryptECB(value.encryptData);
                value = JsonConvert.DeserializeObject<Criteria>(decryptData);

                var col = new Database().MongoClient<BsonDocument>("externalApi");
                var filter = Builders<BsonDocument>.Filter.Eq("officeRequest", value.officeRequest) & Builders<BsonDocument>.Filter.Eq("officeKey", value.officeKey);
                var doc = col.Find(filter).Any();

                if (!doc)
                    throw new Exception("not found office.");

                //return new Response { objectData = value };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            try
            {
                //value.statisticsCreate("news");
                var col = new Database().MongoClient<News>("news");
                var filter = Builders<News>.Filter.Eq("status", "A");

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<News>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter &= Builders<News>.Filter.Regex("title", value.keySearch);

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        _ = value1.statisticsCreateAsync("newsKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }

                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<News>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<News>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter &= Builders<News>.Filter.Eq("isHighlight", value.isHighlight); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", ds.start) & Builders<News>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<News>.Filter.Gt("docDate", de.start) & Builders<News>.Filter.Lt("docDate", de.end); }

                List<News> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("newsCategory", "category", "code", "categoryList")
                                      .Lookup("register", "createBy", "username", "userList")
                                      .As<News>()
                                      .ToList();

                //var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        new StatisticService(value.code, docs[0].title, "News", value.profileCode, "", "");

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docs.Count > 0 ? docs[0].categoryList[0].title : "";
                    }

                    _ = value.statisticsCreateAsync("news");
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

                    _ = valueCategory.statisticsCreateAsync("newsCategory");
                }
                catch { }
                //END : Statistic

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient( "news");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                          .Lookup("newsCategory", "category", "code", "categoryList")
                                          .Lookup("register", "createBy", "username", "userList")
                                          .As<News>()
                                          .ToList();

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.imageUrl, c.category, c.title, c.language, c.description, c.titleEN, c.descriptionEN, c.view, c.createDate, c.createBy, c.imageUrlCreateBy }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}