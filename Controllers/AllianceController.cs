using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Linq;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AllianceController : ControllerBase
    {
        public AllianceController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Alliance value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("alliance");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "description", value.description },
                    { "descriptionEN", value.descriptionEN },
                    { "imageUrl", value.imageUrl },
                    { "linkUrl", value.linkUrl},
                    { "action", value.action },
                    { "imageSize", value.imageSize },
                    { "note", value.note },
                    { "mainPage", value.mainPage },
                    { "contactPage", value.contactPage },
                    { "newsPage", value.newsPage },
                    { "eventPage", value.eventPage },
                    { "knowledgePage", value.knowledgePage },
                    { "lawPage", value.lawPage },
                    { "personnelPage", value.personnelPage },
                    { "imageEventPage", value.imageEventPage },
                    { "importantPage", value.importantPage },
                    { "knowledgeVetPage", value.knowledgeVetPage },
                    { "vetEnewsPage", value.vetEnewsPage },
                    { "expertBranchPage", value.expertBranchPage },
                    { "trainingInstitutePage", value.trainingInstitutePage },
                    { "verifyApprovedUserPage", value.verifyApprovedUserPage },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
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
                    { "isPostHeader", value.isPostHeader },
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
                var col = new Database().MongoClient<Alliance>("alliance");
                var filter = Builders<Alliance>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Alliance>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Alliance>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.page)) { filter &= Builders<Alliance>.Filter.Eq("isActive", true); }
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Alliance>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<Alliance>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter &= Builders<Alliance>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Alliance>.Filter.Eq("sequence", sequence); }
                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Alliance>.Filter.Gt("docDate", ds.start) & Builders<Alliance>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Alliance>.Filter.Gt("docDate", ds.start) & Builders<Alliance>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Alliance>.Filter.Gt("docDate", de.start) & Builders<Alliance>.Filter.Lt("docDate", de.end); }
                }
                var docs = col.Find(filter).SortBy(o => o.sequence).Skip(value.skip).Limit(value.limit).Project(c => new {
                    c.code,
                    c.createDate,
                    c.createBy,
                    c.updateDate,
                    c.updateBy,
                    c.docDate,
                    c.docTime,
                    c.isActive,
                    c.sequence,
                    c.title,
                    c.titleEN,
                    c.imageUrl,
                    c.linkUrl,
                    c.description,
                    c.imageUrlCreateBy,
                    c.descriptionEN,
                    c.action,
                    c.note,
                    c.isPostHeader,
                    c.imageSize,
                    c.mainPage,
                    c.newsPage,
                    c.eventPage,
                    c.imageEventPage,
                    c.knowledgePage,
                    c.lawPage,
                    c.personnelPage,
                    c.contactPage,
                    c.importantPage,
                    c.knowledgeVetPage,
                    c.vetEnewsPage,
                    c.expertBranchPage,
                    c.verifyApprovedUserPage,
                    c.trainingInstitutePage,
                }).ToList();

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Alliance value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("alliance");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.titleEN)) { doc["titleEN"] = value.titleEN; }
                if (!string.IsNullOrEmpty(value.linkUrl)) { doc["linkUrl"] = value.linkUrl; }
                if (!string.IsNullOrEmpty(value.description)) { doc["description"] = value.description; }
                if (!string.IsNullOrEmpty(value.descriptionEN)) { doc["descriptionEN"] = value.descriptionEN; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.action)) { doc["action"] = value.action; }
                if (!string.IsNullOrEmpty(value.note)) { doc["note"] = value.note; }
                if (!string.IsNullOrEmpty(value.imageSize)) { doc["imageSize"] = value.imageSize; }


                doc["mainPage"] = value.mainPage;
                doc["contactPage"] = value.contactPage;
                doc["newsPage"] = value.newsPage;
                doc["eventPage"] = value.eventPage;
                doc["knowledgePage"] = value.knowledgePage;
                doc["lawPage"] = value.lawPage;
                doc["personnelPage"] = value.personnelPage;
                doc["importantPage"] = value.importantPage;
                doc["imageEventPage"] = value.imageEventPage;
                doc["knowledgeVetPage"] = value.knowledgeVetPage;
                doc["vetEnewsPage"] = value.vetEnewsPage;
                doc["expertBranchPage"] = value.expertBranchPage;
                doc["trainingInstitutePage"] = value.trainingInstitutePage;
                doc["verifyApprovedUserPage"] = value.verifyApprovedUserPage;
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                doc["isPostHeader"] = value.isPostHeader;

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
        public ActionResult<Response> Delete([FromBody] Alliance value)
        {
            try
            {
                var col = new Database().MongoClient("alliance");
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

        #endregion

        #region gallery

        // POST /create
        [HttpPost("gallery/create")]
        public ActionResult<Response> GalleryCreate([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("allianceGallery");

                value.code = "".toCode();

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "reference", value.reference },
                    { "isActive", true }
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
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("bannerGallery");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                        var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                        col.UpdateMany(filter, update);
                    }
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion
    }
}
