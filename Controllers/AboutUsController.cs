using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class AboutUsController : Controller
    {
        public AboutUsController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] AboutUs value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("aboutUs");
                var colRegister = new Database().MongoClient<Register>("register");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "imageBgUrl", value.imageBgUrl },
                    { "imageLogoUrl", value.imageLogoUrl },
                    { "description", value.description.ConvertStrToHtml() },
                    { "descriptionEN", value.descriptionEN.ConvertStrToHtml() },
                    { "latitude", value.latitude},
                    { "longitude", value.longitude},
                    { "address", value.address},
                    { "addressEN", value.addressEN},
                    { "telephone", value.telephone},
                    { "email", value.email},
                    { "site", value.site},
                    { "facebook", value.facebook},
                    { "lineOfficial", value.lineOfficial},
                    { "vision", value.vision },
                    { "visionEN", value.visionEN },
                    { "missionList", new BsonArray(
                        value.missionList.Select((m, index) => new BsonDocument
                        {
                            { "sequence", index + 1 },
                            { "title", m.title },
                            { "titleEN", m.titleEN }
                        }))
                    },
                    { "youtube", value.youtube},
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "center", colRegister.Find(o => o.username == value.updateBy).FirstOrDefault().center ?? "" },
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
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<AboutUs>("aboutUs");
                var filter = Builders<AboutUs>.Filter.Eq("isActive", true);

                var codeCenter = col.Find(Builders<AboutUs>.Filter.Eq("code", value.code)).Project(c => c.center).FirstOrDefault();

                //if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<AboutUs>.Filter.Eq("code", value.code); }
                if (!string.IsNullOrEmpty(codeCenter)) { filter = filter & Builders<AboutUs>.Filter.Eq("center", codeCenter); }
                else { filter = filter & Builders<AboutUs>.Filter.Eq("center", ""); }
                var docs = col.Find(filter).Project(c => new { c.code, c.isActive, c.title,c.titleEN, c.imageLogoUrl, c.imageBgUrl, c.description, c.descriptionEN, c.vision, c.visionEN, c.latitude, c.email, c.site, c.longitude, c.address,c.addressEN, c.facebook, c.youtube, c.telephone, c.createBy, c.createDate, c.updateBy, c.updateDate, c.lineOfficial, missionList = (c.missionList ?? Enumerable.Empty<Identity>()).Select(x => new {sequence = x.sequence, title = x.title, titleEN = x.titleEN} ) }).ToList();
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] AboutUs value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("aboutUs");
                var colRegister = new Database().MongoClient<Register>("register");

                if (!string.IsNullOrEmpty(value.code))
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["title"] = value.title ?? "";
                    doc["titleEN"] = value.titleEN ?? "";
                    doc["imageBgUrl"] = value.imageBgUrl ?? "";
                    doc["imageLogoUrl"] = value.imageLogoUrl ?? "";
                    doc["description"] = value.description.ConvertStrToHtml() ?? "";
                    doc["descriptionEN"] = value.descriptionEN.ConvertStrToHtml() ?? "";
                    doc["latitude"] = value.latitude ?? "";
                    doc["longitude"] = value.longitude ?? "";
                    doc["address"] = value.address ?? "";
                    doc["addressEN"] = value.addressEN ?? "";
                    doc["telephone"] = value.telephone ?? "";
                    doc["email"] = value.email ?? "";
                    doc["facebook"] = value.facebook ?? "";
                    doc["lineOfficial"] = value.lineOfficial ?? "";
                    doc["youtube"] = value.youtube ?? "";
                    doc["site"] = value.site ?? "";
                    doc["updateBy"] = value.updateBy ?? "";
                    doc["vision"] = value.vision ?? "";
                    doc["visionEN"] = value.visionEN ?? "";
                    doc["missionList"] = new BsonArray(
                        value.missionList.Select((m, index) => new BsonDocument
                        {
                            { "sequence", index + 1 },
                            { "title", m.title },
                            { "titleEN", m.titleEN }
                        }));

                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                    doc["center"] = colRegister.Find(o => o.username == value.updateBy).FirstOrDefault().center ?? "";
                    col.ReplaceOne(filter, doc);
                }
                else
                {
                    doc = new BsonDocument
                    {
                        { "code", value.code },
                        { "title", value.title  ?? ""},
                        { "imageLogoUrl", value.imageLogoUrl  ?? ""},
                        { "imageBgUrl", value.imageBgUrl  ?? ""},
                        { "description", value.description.ConvertStrToHtml()  ?? ""},
                        { "descriptionEN", value.descriptionEN.ConvertStrToHtml()  ?? ""},
                        { "latitude", value.latitude  ?? ""},
                        { "longitude", value.longitude  ?? ""},
                        { "address", value.address  ?? ""},
                        { "telephone", value.telephone  ?? ""},
                        { "email", value.email  ?? ""},
                        { "site", value.site  ?? ""},
                        { "facebook", value.facebook  ?? ""},
                        { "lineOfficial", value.lineOfficial  ?? ""},
                        { "vision", value.vision  ?? ""},
                        { "visionEN", value.visionEN  ?? ""},
                        { "missionList", new BsonArray(
                            value.missionList.Select((m, index) => new BsonDocument
                            {
                                { "sequence", index + 1 },
                                { "title", m.title },
                                { "titleEN", m.titleEN }
                            }))
                        },
                        { "youtube", value.youtube  ?? ""},
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "docDate", DateTime.Now },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "center", colRegister.Find(o => o.username == value.updateBy).FirstOrDefault().center ?? "" },
                    };
                    col.InsertOne(doc);
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] AboutUs value)
        {
            try
            {
                var col = new Database().MongoClient("aboutUs");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        // POST /read
        [HttpPost("readComment")]
        public ActionResult<Response> readComment([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<EventCalendar>("aboutUsComment");
                var filter = Builders<EventCalendar>.Filter.Ne("status", "D");

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new
                {
                    firstName = c.firstName,
                    lastName = c.lastName,
                    email = c.email,
                    phone = c.phone,
                    title = c.title,
                    description = c.description,
                    fileUrl = c.fileUrl,
                    createDate = c.createDate,
                }).ToList();


                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs,totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

    }
}