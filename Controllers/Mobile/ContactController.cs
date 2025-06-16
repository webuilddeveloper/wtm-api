using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class ContactController : Controller
    {
        public ContactController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                //value.statisticsCreateAsync("contact");
                var col = new Database().MongoClient<Contact>("contact");
                var filter = (Builders<Contact>.Filter.Eq("status", "A"));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"));

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("contactKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Contact>.Filter.Eq("code", value.code); }
                else if (!string.IsNullOrEmpty(value.center)) { filter = filter & Builders<Contact>.Filter.Eq("center", value.center); }
                else { filter = filter & Builders<Contact>.Filter.Eq("center", ""); }
                if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Contact>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Contact>.Filter.Regex("language", value.language); }

                List<Contact> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new Contact{
                    code = c.code,
                    category = c.category,
                    isActive = c.isActive,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    createDate = c.createDate,
                    imageUrl = c.imageUrl,
                    title = c.title,
                    language = c.language,
                    updateBy = c.updateBy,
                    updateDate = c.updateDate,
                    sequence = c.sequence,
                    phone = c.phone,
                    note = c.note
                }).ToList();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        //Get Category
                        var colCategory = new Database().MongoClient<Contact>("contactCategory");
                        var filterCategory = Builders<Contact>.Filter.Eq("code", docs[0].category);
                        Category docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code, title = c.title }).FirstOrDefault();

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docCategory.title;

                        value.statisticsCreateAsync("contact");
                    }

                }
                catch { }

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
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
                var col = new Database().MongoClient<Category>("contactCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.phone)) { filter = filter & Builders<Category>.Filter.Regex("phone", value.phone); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<suggestionCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<suggestionCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
    }
}