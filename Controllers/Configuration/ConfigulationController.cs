using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace configulation_api.Controllers
{
    [Route("[controller]")]
    public class ConfigulationController : Controller
    {
        public ConfigulationController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Register value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("configulation");
                value.code = "".toCode();
                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "username", value.username },
                    { "description", value.description },
                    { "email", value.email },
                    { "password", value.password },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /create
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Register value)
        {
            try
            {
                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var filter = Builders<BsonDocument>.Filter.Eq("isActive", true);
                if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<BsonDocument>.Filter.Eq("title", value.title); }
                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<BsonDocument>.Filter.Eq("username", value.username); }
                if (!string.IsNullOrEmpty(value.email)) { filter &= Builders<BsonDocument>.Filter.Eq("email", value.email); }
                if (!string.IsNullOrEmpty(value.password)) { filter &= Builders<BsonDocument>.Filter.Eq("password", value.password); }
                var colConfig = new Database().MongoClient("configulation");
                var docConfig = colConfig.Find(filter).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = "", objectData = BsonSerializer.Deserialize<object>(docConfig) };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("configulation");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.username)) { doc["username"] = value.username; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                if (!string.IsNullOrEmpty(value.password)) { doc["password"] = value.password; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Contact value)
        {
            try
            {
                var col = new Database().MongoClient("configulation");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("shared/read")]
        public ActionResult<Response> SharedRead([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "shared");
                var colConfig = new Database().MongoClient("configulation");
                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = "", objectData = BsonSerializer.Deserialize<object>(docConfig) };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("email/read")]
        public ActionResult<Response> MailRead([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var newPassword = "".getRandom();
                var col = new Database().MongoClient("register");
                var filterConfig = Builders<BsonDocument>.Filter.Eq("title", "email");

                var colConfig = new Database().MongoClient("configulation");

                var docConfig = colConfig.Find(filterConfig).FirstOrDefault();

                return new Response { status = "s", message = "success", jsonData = docConfig.ToJson(), objectData = BsonSerializer.Deserialize<object>(docConfig) };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("email/create")]
        public ActionResult<Response> ForgetReadAsync([FromBody] Register value)
        {
            try
            {

                var doc = new BsonDocument();
                var col = new Database().MongoClient("configulation");

                var filter = Builders<BsonDocument>.Filter.Eq("title", "email");
                if (col.Find(filter).Any())
                {
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                    if (!string.IsNullOrEmpty(value.username)) { doc["username"] = value.username; }
                    if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                    if (!string.IsNullOrEmpty(value.password)) { doc["password"] = value.password; }
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                    doc["isActive"] = value.isActive;
                    col.ReplaceOne(filter, doc);
                }
                else
                {
                    doc = new BsonDocument
                    {
                    { "code", "".toCode() },
                    { "title", value.title },
                    { "username", value.username },
                    { "email", value.email },
                    { "password", value.password },
                    { "createBy", value.createBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive }
                    };
                    col.InsertOne(doc);
                }

                return new Response { status = "s", message = "success", jsonData = value.ToJson(), objectData = value };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpGet("initialEmail")]
        public ActionResult<Response> InitialEmail()
        {
            try
            {
                {
                    var doc = new BsonDocument();
                    var col = new Database().MongoClient("configulation");
                    doc = new BsonDocument
                        {
                        { "code", "".toCode() },
                        { "title", "email" },
                        { "username", "webuild" },
                        { "email", "ext18979@gmail.com" },
                        { "password", "EX74108520" },
                        { "description", "" },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                        };
                    col.InsertOne(doc);
                }

                {
                    var doc = new BsonDocument();
                    var col = new Database().MongoClient("configulation");
                    doc = new BsonDocument
                        {
                        { "code", "".toCode() },
                        { "title", "shared" },
                        { "username", "" },
                        { "email", "" },
                        { "password", "" },
                        { "description", "http://shared.we-builds.com/opec/" },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                        };
                    col.InsertOne(doc);
                }



                return new Response { status = "s", message = "success", jsonData = "", objectData = new { } };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }

    #endregion
}
