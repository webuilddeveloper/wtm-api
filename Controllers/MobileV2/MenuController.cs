using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Models;
using cms_api.Extension;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobilev2_api.Controllers
{
    [Route("m/v2/[controller]")]
    public class MenuController : Controller
    {

        public MenuController() { }

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> CommentCreate([FromBody] List<News> value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("menu2");
                col.DeleteMany(_ => true);

                value.ForEach(c =>
                {
                    doc = new BsonDocument
                    {
                        { "code", c.code },
                        { "title", c.title },
                        { "imageUrl", c.imageUrl },
                        { "createBy", "System" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "System" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "status", "A" }
                    };
                    col.InsertOne(doc);
                });

                return new Response { status = "S", message = "success", objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<News>("menu2");
                var filter = Builders<News>.Filter.Eq("status", "A");
                var docs = col.Find(filter).Project(c => new { c.code, c.title, c.imageUrl }).ToList();

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        

    }
}