using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class VeterinaryController : Controller
    {
        public VeterinaryController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] List<Register> values)
        {
            var doc = new BsonDocument();

            try
            {
                int count = 0;
                foreach (var value in values)
                {
                    count++;
                    var col = new Database().MongoClient("m_Veterinary");

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
                        { "sequence", count },
                        { "prefixName", value.prefixName },
                        { "firstName", value.firstName },
                        { "lastName", value.lastName },
                        { "category", value.category },
                        { "licenseNumber",value.licenseNumber },
                        { "dateOfIssue", value.dateOfIssue },
                        { "dateOfExplry", value.dateOfExplry },
                        { "reNewTo", value.reNewTo },
                        { "address", value.address },
                        { "tambon", value.tambon },
                        { "amphoe", value.amphoe },
                        { "province", value.province },
                        { "postnoCode", value.postnoCode },
                        { "phone", value.phone },
                        { "email", value.email },
                        { "idcard", value.idcard },
                        { "codeShortNumber", value.codeShortNumber },


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
                }

                return new Response { status = "S", message = "success", objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                var col = new Database().MongoClient("_log_Veterinary");
                //check duplicate
                var code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                doc = new BsonDocument
                    {
                        { "code", code },
                        { "Message", ex.Message },
                        { "InnerException", ex.InnerException.Message },
                        { "2InnerException", ex.InnerException.InnerException.Message },
                    };
                col.InsertOne(doc);

                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Eq("code", value.profileCode);
                var docsRegister = colRegister.Find(filterRegister).Project(c => new { c.idcard }).FirstOrDefault();



                var col = new Database().MongoClient<Register>("m_Veterinary");
                var filter = Builders<Register>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.firstName) && !string.IsNullOrEmpty(value.lastName))
                {
                    filter &= Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i")) & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i"));
                }
                else if (!string.IsNullOrEmpty(value.firstName))
                {
                    filter &= Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i")) | Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.firstName), "i"));
                }

                //if (!string.IsNullOrEmpty(value.lastName))
                //    filter &= Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.lastName), "i"));
                //    //filter = (filter & Builders<News>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<News>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                if (!string.IsNullOrEmpty(value.title))
                    filter &= (Builders<Register>.Filter.Regex("title", value.title) | Builders<Register>.Filter.Regex("titleEN", value.title));
                //if (!string.IsNullOrEmpty(value.category)) { filter &= Builders<Register>.Filter.Eq("category", value.category); }

                if (!string.IsNullOrEmpty(value.code))
                    filter &= Builders<Register>.Filter.Eq("code", value.code);

                //var docs = col.Find(filter).Project(c => new { c.code, c.title, c.titleEN, c.category, c.createBy, c.createDate, c.updateBy, c.updateDate, c.isActive }).ToList();
                List<Register> docs = col.Aggregate().Match(filter)
                    .SortByDescending(o => o.docDate)
                    .ThenByDescending(o => o.updateTime)
                    .Skip(value.skip)
                    .Limit(value.limit)
                    .As<Register>()
                    .ToList();

                var model = new List<object>();
                docs.ForEach(c =>
                {
                    var isOwner = false; //โชว์วันที่ออกบัตรกับวันหมดอายุ

                    var idcard = c.idcard.Replace("-", "");
                    if (docsRegister != null)
                        if (docsRegister.idcard == idcard)
                            isOwner = true;

                    model.Add(new
                    {
                        isOwner = isOwner,
                        code = c.code,
                        sequence = c.sequence,
                        licenseNumber = c.licenseNumber,
                        reNewTo = c.reNewTo,
                        idcard = c.idcard,
                        //imageUrl = c.imageUrl,
                        imageUrl = "https://vet.we-builds.com/vet-document/images/vet/" + (c.category == "ชั้นหนึ่ง" ? "1" : "2") + "/" + c.codeShortNumber + ".jpg",
                        prefixName = c.prefixName,
                        firstName = c.firstName,
                        lastName = c.lastName,
                        category = c.category,
                        dateOfIssue = c.dateOfIssue,
                        dateOfExplry = c.dateOfExplry,
                        address = c.address,
                        tambon = c.tambon,
                        amphoe = c.amphoe,
                        province = c.province,
                        postnoCode = c.postnoCode,
                        phone = c.phone,
                        email = c.email,
                        codeShortNumber = c.codeShortNumber,

                        isActive = c.isActive,
                        status = c.status,
                    });
                });

                return new Response { status = "S", message = "success", objectData = model, totalData = col.Find(filter).ToList().Count() };
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
                var col = new Database().MongoClient("m_Veterinary");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                doc["sequence"] = value.sequence;
                doc["prefixName"] = value.prefixName;
                doc["firstName"] = value.firstName;
                doc["lastName"] = value.lastName;
                doc["category"] = value.category;
                doc["licenseNumber"] = value.licenseNumber;
                doc["dateOfIssue"] = value.dateOfIssue;
                doc["dateOfExplry"] = value.dateOfExplry;
                doc["reNewTo"] = value.reNewTo;
                doc["address"] = value.address;
                doc["tambon"] = value.tambon;
                doc["amphoe"] = value.amphoe;
                doc["province"] = value.province;
                doc["postnoCode"] = value.postnoCode;
                doc["phone"] = value.phone;
                doc["email"] = value.email;
                doc["idcard"] = value.idcard;
                doc["codeShortNumber"] = value.codeShortNumber;

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";

                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Identity value)
        {
            try
            {
                var col = new Database().MongoClient("m_Veterinary");

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

    }
}