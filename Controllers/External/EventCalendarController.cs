using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace external_api.Controllers
{
    [Route("ext/[controller]")]
    public class EventCalendarController : Controller
    {
        public EventCalendarController() { }

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
                //value.statisticsCreateAsync("eventCalendar");
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = Builders<EventCalendar>.Filter.Eq("status", "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter &= Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"));

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        _ = value1.statisticsCreateAsync("eventCalendarKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<EventCalendar>.Filter.Eq("code", value.code); }
                else if (!string.IsNullOrEmpty(value.center)) { filter &= Builders<EventCalendar>.Filter.Eq("center", value.center); }
                else { filter &= Builders<EventCalendar>.Filter.Eq("center", ""); }
                if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<EventCalendar>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter &= Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter &= Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description))); }
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<EventCalendar>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter &= Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                

                List<EventCalendar> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new EventCalendar{
                    code = c.code,
                    isActive = c.isActive,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    createDate = c.createDate,
                    description = c.description,
                    imageUrl = c.imageUrl,
                    title = c.title,
                    language = c.language,
                    updateBy = c.updateBy,
                    updateDate = c.updateDate,
                    category = c.category,
                    confirmStatus = c.confirmStatus,
                    linkFacebook = c.linkFacebook,
                    linkYoutube = c.linkYoutube,
                    dateStart = c.dateStart,
                    dateEnd = c.dateEnd,
                    view = c.view,
                    linkUrl = c.linkUrl,
                    textButton = c.textButton,
                    fileUrl = c.fileUrl }).ToList();
                
                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient( "eventCalendar");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs[0].view += 1;

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        //Get Category
                        var colCategory = new Database().MongoClient<Contact>("eventCalendarCategory");
                        var filterCategory = Builders<Contact>.Filter.Eq("code", docs[0].category);
                        Category docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code, title = c.title }).FirstOrDefault();

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docCategory.title;

                        _ = value.statisticsCreateAsync("eventCalendar");
                    }

                }
                catch { }

                //endStatistic

                // get User: firstname lastname, imageurl
                docs.ForEach(c =>
                {
                    try
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Eq("username", c.updateBy);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.imageUrlCreateBy = docRegister.imageUrl;
                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                    }
                    catch
                    {

                    }
                });


                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }

}