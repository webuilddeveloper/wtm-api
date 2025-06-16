using System;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class AboutUsController : Controller
    {
        public AboutUsController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<AboutUs>("aboutUs");

                var filter = Builders<AboutUs>.Filter.Eq("code", "1");
                //filter = filter | Builders<AboutUs>.Filter.Eq("isActive", false);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<AboutUs>.Filter.Regex("code", value.code); }

                var docs = col.Find(filter).Project(c => new { c.code, c.isActive, c.title, c.imageLogoUrl, c.imageBgUrl, c.description, c.latitude, c.email, c.site, c.longitude, c.address, c.facebook, c.youtube, c.telephone, c.createBy, c.createDate, c.updateBy, c.updateDate, c.lineOfficial }).FirstOrDefault();

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(docs.code))
                    {
                        value.reference = value.code;
                        value.title = docs.title;
                        value.category = "";

                        value.statisticsCreateAsync("aboutUs");
                    }

                }
                catch { }
                //END : Statistic

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        [HttpPost("create")]
        public ActionResult<Response> create([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient("aboutUsComment");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                var doc = new BsonDocument
                {
                    { "code", value.code },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "email", value.email },
                    { "phone", value.phone },
                    { "title", value.title},
                    { "description", value.description},
                    { "fileUrl", value.fileUrl},
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive },
                    { "status", value.isActive ? "A" : "N" }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success"};
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}