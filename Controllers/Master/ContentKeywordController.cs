using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OfficeOpenXml;

namespace master_api.Controllers
{
    public class ContentKeyword
    {
        public ContentKeyword()
        {
            sequence = 0;
            code = "";
            title = "";
            keyword = "";
            url = "";
            updateBy = "";
            isActive = false;
        }

        public int sequence { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public string keyword { get; set; }
        public string url { get; set; }
        public string updateDate { get; set; }
        public string updateBy { get; set; }
        public bool isActive { get; set; }
    }

    [Route("[controller]")]
    public class ContentKeywordController : Controller
    {
        public ContentKeywordController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> ContentKeywordRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<ContentKeyword>("mContentKeyword");
                var filter = Builders<ContentKeyword>.Filter.Eq("isActive", true);

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter &= Builders<ContentKeyword>.Filter.Regex("title", value.keySearch);
                }


                if (!string.IsNullOrEmpty(value.code))
                {
                    filter &= Builders<ContentKeyword>.Filter.Eq("code", value.code);
                }

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new {c.sequence, c.code, c.title,c.keyword,c.url,c.updateBy,c.updateDate }).ToList();

                return new Response { status = "S", message = "success", jsonData = "", objectData = docs, totalData = col.Find(filter).Project(c => new { c.code, c.title }).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> ContentKeywordCreate([FromBody] ContentKeyword value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("mContentKeyword");

                {
                    //check duplicate
                    value.code = "".toCode();
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                {
                    //check title
                    var filter = Builders<BsonDocument>.Filter.Eq("title", value.title);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"title: {value.title} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }
                

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "keyword", value.keyword },
                    { "url", value.url },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "S", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> ContentKeywordUpdate([FromBody] ContentKeyword value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("mContentKeyword");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                doc["sequence"] = value.sequence;
                doc["title"] = value.title;
                doc["keyword"] = value.keyword;
                doc["url"] = value.url;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;

                col.ReplaceOne(filter, doc);


                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "S", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] News value)
        {
            try
            {
                var col = new Database().MongoClient("mContentKeyword");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        [HttpPost("readContentKeyword")]
        public ActionResult<Response> readContentKeyword([FromBody] ContentKeyword value)
        {
            try
            {
                var content = new List<News>();

                var col = new Database().MongoClient<ContentKeyword>("mContentKeyword");
                var filter = Builders<ContentKeyword>.Filter.Eq("isActive", true);

                //if (!string.IsNullOrEmpty(value.keyword))
                //{
                //    filter &= Builders<ContentKeyword>.Filter.Regex("keyword", value.keyword);
                //}

                var option = new AggregateOptions() { AllowDiskUse = true };
                var docs = col.Aggregate(option).Match(filter).Project(c => new ContentKeyword { sequence = c.sequence, code= c.code, title = c.title, keyword=  c.keyword, url = c.url, updateBy = c.updateBy, updateDate = c.updateDate }).ToList();

                if (!string.IsNullOrEmpty(value.keyword))
                    docs = docs.Where(c => docs.Select(s => new { code = s.code, keyword = s.keyword.Split(',').Where(c => c.Contains(value.keyword)) }).Where(w => w.keyword.Count() > 0).Select(ss => ss.code).Contains(c.code) ).ToList();

                content.AddRange(getconten("news", value.keyword));
                content.AddRange(getconten("eventCalendar", value.keyword));
                //content.AddRange(getconten("contact", value.keyword));
                content.AddRange(getconten("knowledge", value.keyword));
                content.AddRange(getconten("knowledge-vet", value.keyword));
                //content.AddRange(getconten("privilege", value.keyword));
                //content.AddRange(getconten("poll", value.keyword));
                //content.AddRange(getconten("reporter", value.keyword));
                //content.AddRange(getconten("cooperativeForm", value.keyword));
                //content.AddRange(getconten("examination", value.keyword));
                content.AddRange(getconten("important", value.keyword));
                content.AddRange(getconten("imageEvent", value.keyword));
                content.AddRange(getconten("vetEnews", value.keyword));

                var respone = new {
                    keyword = docs,
                    content = content,
                };

                return new Response { status = "S", message = "success", jsonData = "", objectData = respone };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        public List<News> getconten(string clinent,string keywords)
        {
            var col = new Database().MongoClient<News>(clinent);
            var filter = (Builders<News>.Filter.Eq("status", "A") & Builders<News>.Filter.Eq("isActive", true));

            if (!string.IsNullOrEmpty(keywords))
            {
                filter = (filter & Builders<News>.
                        Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", keywords), "i"))) | (filter & Builders<News>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", keywords), "i")));

            }

            var option = new AggregateOptions() { AllowDiskUse = true };
            var docs = col.Aggregate(option).Match(filter).Project(c => new News
            {
                code = c.code,
                title = c.title,
                imageUrl = c.imageUrl,
                category = c.category,
                description = c.description,
                fileUrl = c.fileUrl,
                linkUrl = c.linkUrl,
                createBy = c.createBy,
                createDate = c.createDate,
                createTime = c.createTime,
                updateBy = c.updateBy,
                updateDate = c.updateDate,
                updateTime = c.updateTime,
                docDate = c.docDate,
                docTime = c.docTime,
                cpCode = clinent,
                //code = c["code"],
                //title = c["title"],
                //imageUrl = c["imageUrl"],
                //category = c["category"],
                //description = c["description"],
                //fileUrl = c["fileUrl"],
                //linkUrl = c["linkUrl"],
                //createBy = c["createBy"],
                //createDate = c["createDate"],
                //createTime = c["createTime"],
                //updateBy = c["updateBy"],
                //updateDate = c["updateDate"],
                //updateTime = c["updateTime"],
                //docDate = c["docDate"],
                //docTime = c["docTime"],
            }).ToList();


            return docs;
        }
    }
}