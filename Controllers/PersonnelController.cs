using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cms_api.Extension;
using cms_api.Models;
using Jose;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class PersonnelController : Controller
    {
        public PersonnelController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("personnel");
                var colRegister = new Database().MongoClient<Register>("register");
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                {
                    var filter = Builders<BsonDocument>.Filter.Eq("username", value.username) & Builders<BsonDocument>.Filter.Ne("status", "D");
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"username= {value.username} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }



                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "sequence", value.sequence },
                    { "row", value.row },
                    { "position",value.position },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "description", value.description },

                    { "positionEN",value.positionEN },
                    { "prefixNameEN", value.prefixNameEN },
                    { "firstNameEN", value.firstNameEN },
                    { "lastNameEN", value.lastNameEN },
                    { "descriptionEN", value.descriptionEN },

                    { "agenda", value.agenda },
                    { "center", colRegister.Find(o => o.username == value.updateBy).FirstOrDefault().center ?? "" },


                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive },
                    { "status", value.isActive ? "A" : "N" },
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
        [HttpPost("read")]
        public ActionResult<Response> MemberRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("personnel");
                var filter = Builders<Register>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.status))
                        filter = filter & Builders<Register>.Filter.Eq("status", value.status);

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Eq("code", value.code); }
                    else if (!string.IsNullOrEmpty(value.center)) { filter = filter & Builders<Register>.Filter.Eq("center", value.center); }
                    else { filter = filter & Builders<Register>.Filter.Eq("center", ""); }
                    if (!string.IsNullOrEmpty(value.firstName)) { filter = filter & Builders<Register>.Filter.Eq("firstName", value.firstName); }
                    if (!string.IsNullOrEmpty(value.lastName)) { filter = filter & Builders<Register>.Filter.Eq("lastName", value.lastName); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Eq("createBy", value.createBy); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }

                    if (value.startTerm > 0) { filter = filter & Builders<Register>.Filter.Gte("agenda", value.startTerm); }
                    if (value.endTerm > 0) { filter = filter & Builders<Register>.Filter.Lt("agenda", value.endTerm); }

                }

                var docs = col.Find(filter)
                    .SortByDescending(o => o.docDate)
                    .ThenByDescending(o => o.updateTime)
                    .Skip(value.skip)
                    .Limit(value.limit)
                    .Project(c => new
                    {
                        c.code,
                        c.imageUrl,
                        c.sequence,
                        c.row,
                        c.prefixName,
                        c.firstName,
                        c.lastName,
                        c.description,
                        c.prefixNameEN,
                        c.firstNameEN,
                        c.lastNameEN,
                        c.descriptionEN,
                        c.agenda,
                        c.position,
                        c.positionEN,

                        c.status,
                        c.isActive,
                        c.createBy,
                        c.createDate,
                        c.createTime,
                        c.updateBy,
                        c.updateDate,
                        c.updateTime,
                        c.docDate,
                        c.docTime,

                    }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                //jsonData = docs.ToJson()
                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> UpdateContent([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("personnel");
                var colRegister = new Database().MongoClient<Register>("register");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                doc["imageUrl"] = value.imageUrl;
                doc["sequence"] = value.sequence;
                doc["row"] = value.row;
                doc["position"] = value.position;
                doc["prefixName"] = value.prefixName;
                doc["firstName"] = value.firstName;
                doc["lastName"] = value.lastName;

                doc["positionEN"] = value.positionEN;
                doc["prefixNameEN"] = value.prefixNameEN;
                doc["firstNameEN"] = value.firstNameEN;
                doc["lastNameEN"] = value.lastNameEN;

                doc["agenda"] = value.agenda;
                doc["description"] = value.description;
                doc["descriptionEN"] = value.descriptionEN;
                doc["center"] = colRegister.Find(o => o.username == value.updateBy).FirstOrDefault().center ?? "";

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
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
        public ActionResult<Response> MemberDelete([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient("personnel");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion
 }
}