using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Jose;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace mobilev2_api.Controllers
{
    [Route("m/v2/[controller]")]
    public class RegisterController : Controller
    {
        public RegisterController() { }

        // POST /login
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Register value)
        {
            var doc1 = new BsonDocument();
            try
            {
                var colVet = new Database().MongoClient<Register>("m_Veterinary");
                var filterVet = Builders<Register>.Filter.Eq("isActive", true) & Builders<Register>.Filter.Ne("idcard", "");
                var docVetTolist = colVet.Find(filterVet).Project(c => new { c.dateOfExplry, c.reNewTo, c.idcard }).ToList();

                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");

                if (!string.IsNullOrEmpty(value.code)) filter &= Builders<Register>.Filter.Eq("code", value.code);
                if (!string.IsNullOrEmpty(value.profileCode)) filter &= Builders<Register>.Filter.Eq("code", value.profileCode);

                int expire_date = 90;

                var sv = new AES();
                //var encryptPassword = "";
                //if (!string.IsNullOrEmpty(value.password))
                //    encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                var doc = col.Find(filter).Project(c => new
                {
                    c.code
                ,
                    c.prefixName
                ,
                    c.username
                ,
                    password = sv.AesEncryptECB(c.password, "p3s6v8y/B?E(H+Mb") 
                ,
                    c.category
                ,
                    c.imageUrl
                ,
                    c.idcard
                ,
                    c.firstName
                ,
                    c.lastName
                ,
                    c.birthDay
                ,
                    c.phone
                ,
                    c.email
                ,
                    countUnit = string.IsNullOrEmpty(c.countUnit) || c.countUnit != "{}" || c.countUnit != "[]" ? JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<CountUnit>>(c.countUnit).Where(c => c.status != "D").ToList()) : ""
                ,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4
                ,
                    c.lv0List,
                    c.lv1List,
                    c.lv2List,
                    c.lv3List,
                    c.lv4List
                ,
                    c.tambonCode,
                    c.tambon,
                    c.amphoeCode,
                    c.amphoe
                ,
                    c.provinceCode,
                    c.province,
                    c.postnoCode,
                    c.postno
                ,
                    c.vetImageUrl,
                    c.vetCategory
                ,
                    dateOfExplry = !string.IsNullOrEmpty(c.idcard) ? docVetTolist.Any(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")) ? !string.IsNullOrEmpty(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).dateOfExplry) ? docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).dateOfExplry : "" : "" : ""
                ,
                    reNewTo = !string.IsNullOrEmpty(c.idcard) ? docVetTolist.Any(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")) ? !string.IsNullOrEmpty(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo) ? docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo : "" : "" : ""
                ,
                    isExpireDate = !string.IsNullOrEmpty(c.idcard) ? docVetTolist.Any(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")) ? !string.IsNullOrEmpty(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo) ? (DateTime.ParseExact(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo, "dd/MM/yy", CultureInfo.InvariantCulture) - DateTime.Now).TotalDays <= expire_date ? "1" : "0" : "0" : "0" : "0"
                ,
                    totalExpireDate = !string.IsNullOrEmpty(c.idcard) ? docVetTolist.Any(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")) ? !string.IsNullOrEmpty(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo) ? (DateTime.ParseExact(docVetTolist.FirstOrDefault(x => x.idcard.Replace("-", "") == c.idcard.Replace("-", "")).reNewTo, "dd/MM/yy", CultureInfo.InvariantCulture) - DateTime.Now).TotalDays : 0 : 0 : 0
                ,
                }).FirstOrDefault();
                //if(doc.birthDay.ToUpper() == "INVALID")
                //{
                //    doc.birthDay = "";
                //}

                if(doc.isExpireDate == "1")
                {
                    var Noti = new Database().MongoClient("notification");
                    var filterNoti = Builders<BsonDocument>.Filter.Eq("isActive", true) & Builders<BsonDocument>.Filter.Eq("category", "expireDate");
                    if (!Noti.Find(filterNoti).Any())
                    {
                        var docNoti = new BsonDocument();

                        docNoti = new BsonDocument
                    {
                    { "code", "".toCode() },
                    { "sequence", 0 },
                    { "title", "ใบอนุญาตหมดอายุ" },
                    { "category", "expireDate" },
                    { "total", 0 },
                    //{ "page", value.page },
                    { "language", "th"},
                    { "fileUrl", ""},
                    { "description", ""},
                    { "reference", doc.code },
                    { "imageUrlCreateBy", "" },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "status",  "A"  },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 }
                    };
                        Noti.InsertOne(docNoti);
                    }
                }             
                var colNoti = new Database().MongoClient<Register>("notification");
                var filtercolNoti = Builders<Register>.Filter.Eq("isActive", true) & Builders<Register>.Filter.Eq("category", "expireDate");
                var codeNoti = colNoti.Find(filtercolNoti).Project(c => c.code).FirstOrDefault();
                if (!string.IsNullOrEmpty(codeNoti))
                {
                    var colNotiSend = new Database().MongoClient<Register>("notificationSend");
                    var col1 = new Database().MongoClient("notificationSend");


                    var filterNotiSend = Builders<Register>.Filter.Eq("reference", codeNoti) & Builders<Register>.Filter.Eq("username", doc.username) & Builders<Register>.Filter.Ne("status", "D");
                    var docNotiSend = colNotiSend.Find(filterNotiSend).Project(c => c.code).FirstOrDefault();
                    if (!string.IsNullOrEmpty(docNotiSend))
                    {
                        if(doc.isExpireDate != "1")
                        {
                            var filter1 = Builders<BsonDocument>.Filter.Eq("reference", codeNoti) & Builders<BsonDocument>.Filter.Eq("username", doc.username) & Builders<BsonDocument>.Filter.Ne("status", "D");
                            var update = Builders<BsonDocument>.Update.Set("status", "D").Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                            col1.UpdateOne(filter1, update);
                        }
                    }
                    else
                    {
                        if (doc.isExpireDate == "1")
                        {
                            var notiSend = new Database().MongoClient("notificationSend");
                            doc1 = new BsonDocument
                            {
                                { "code", "".toCode() },
                                { "username", doc.username },
                                { "category", doc.category },
                                { "reference", codeNoti },
                                { "imageUrlCreateBy", "" },
                                { "createBy", value.updateBy },
                                { "createDate", DateTime.Now.toStringFromDate() },
                                { "createTime", DateTime.Now.toTimeStringFromDate() },
                                { "updateBy", value.updateBy },
                                { "updateDate", DateTime.Now.toStringFromDate() },
                                { "updateTime", DateTime.Now.toTimeStringFromDate() },
                                { "docDate", DateTime.Now.Date.AddHours(7) },
                                { "docTime", DateTime.Now.toTimeStringFromDate() },
                                { "isActive", true },
                                { "status", "N" },
                                { "lv0", value.lv0 },
                                { "lv1", value.lv1 },
                                { "lv2", value.lv2 },
                                { "lv3", value.lv3 },
                                { "lv4", value.lv4 }
                            };
                            col1.InsertOne(doc1);
                        }
                    }

                }
                return new Response { status = "S", message = "success", objectData = doc };
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
                value.logCreate("verify/update", value.code);

                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                //var model = BsonSerializer.Deserialize<object>(doc);

                //if (value.category != "guest")
                //{
                //    doc["username"] = value.username;
                //}

                doc["imageUrl"] = value.imageUrl ?? "";
                doc["category"] = value.category ?? "";
                doc["prefixName"] = value.prefixName ?? "";
                doc["firstName"] = value.firstName ?? "";
                doc["lastName"] = value.lastName ?? "";
                doc["birthDay"] = value.birthDay ?? "";
                doc["phone"] = value.phone ?? "";
                doc["email"] = value.email ?? "";
                doc["facebookID"] = value.facebookID ?? "";
                doc["googleID"] = value.googleID ?? "";
                doc["lineID"] = value.lineID ?? "";
                //doc["line"] = value.line;

                var sv = new AES();
                var encryptPassword = "";
                if (!string.IsNullOrEmpty(value.password))
                    encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                doc["password"] = encryptPassword ?? "";
                doc["sex"] = value.sex ?? "";
                doc["soi"] = value.soi ?? "";
                doc["address"] = value.address ?? "";
                doc["moo"] = value.moo ?? "";
                doc["road"] = value.road ?? "";
                doc["tambonCode"] = value.tambonCode ?? "";
                doc["tambon"] = value.tambon ?? "";
                doc["amphoeCode"] = value.amphoeCode ?? "";
                doc["amphoe"] = value.amphoe ?? "";
                doc["provinceCode"] = value.provinceCode ?? "";
                doc["province"] = value.province ?? "";
                doc["postnoCode"] = value.postnoCode ?? "";
                doc["postno"] = value.postno ?? "";
                //doc["job"] = value.job;
                doc["idcard"] = value.idcard ?? "";
                doc["officerCode"] = value.officerCode ?? "";
                doc["licenseNumber"] = value.licenseNumber ?? "";
                //doc["countUnit"] = value.countUnit;
                //doc["lv0"] = value.lv0;
                //doc["lv1"] = value.lv1;
                //doc["lv2"] = value.lv2;
                //doc["lv3"] = value.lv3;
                //doc["lv4"] = value.lv4;
                //doc["linkAccount"] = value.linkAccount;
                doc["description"] = value.description ?? "";
                doc["isActive"] = value.isActive;
                doc["status"] = value.status;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                col.ReplaceOne(filter, doc);

                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq("code", value.code);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    password = sv.AesEncryptECB(c.password, "p3s6v8y/B?E(H+Mb"),
                    c.status,
                    c.isActive,
                    c.createBy,
                    c.createDate,
                    c.imageUrl,
                    c.updateBy,
                    c.updateDate,
                    c.createTime,
                    c.updateTime,
                    c.docDate,
                    c.docTime,
                    c.category,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    c.birthDay,
                    c.phone,
                    c.email,
                    c.facebookID,
                    c.googleID,
                    c.lineID,
                    //c.line,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambon,
                    c.amphoe,
                    c.province,
                    c.postno,
                    c.tambonCode,
                    c.amphoeCode,
                    c.provinceCode,
                    c.postnoCode,
                    //c.job,
                    c.idcard,
                    c.officerCode,
                    c.licenseNumber,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount
                }).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("verify/update")]
        public ActionResult<Response> VerifyUpdate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            var vetImageUrl = "";
            var vetCategory = "ทั่วไป";
            try
            {
                //value.logCreate("verify/update", value.code);

                //if (string.IsNullOrEmpty(value.licenseNumber) || value.licenseNumber.Length < 4)
                //    return new Response { status = "E", message = "กรุณากรอกรูปแบบเลขที่ใบอนุญาตฯ" };

                //if (string.IsNullOrEmpty(value.idcard) || value.idcard.Length != 13)
                //        return new Response { status = "E", message = "กรุณากรอกรูปแบบเลขบัตรประชาชนให้ถูกต้อง" };

                var col2 = new Database().MongoClient<Register>("m_Veterinary");
                var filter2 = Builders<Register>.Filter.Eq("isActive", true);
                var docVetTolist = col2.Find(filter2).Project(c => new { c.idcard , c.codeShort , c.codeShortNumber,c.licenseNumber,c.category,c.dateOfIssue }).ToList();

                var doc2 = docVetTolist.FirstOrDefault(c => c.idcard.Replace("-","") == value.idcard.Replace("-", ""));
                if (doc2 != null)
                {
                    if (!string.IsNullOrEmpty(doc2.dateOfIssue))
                    {
                        
                        var datenow = DateTime.Now.ToString("yyyy", new CultureInfo("en-US"));
                        //สลับตำแหน่งเอาข้างหลังมาไว้ข้างหน้า เพื่อเลือกสองตัวหลัง เพื่อทำการเช็ดวันที่
                        var switchstr = doc2.dateOfIssue.ToCharArray();
                        Array.Reverse(switchstr);
                        var switchDOI = new string(switchstr).Substring(0, 2).ToCharArray();
                        Array.Reverse(switchDOI);

                        //วันที่ออกบัตร เป็น พศ
                        var year = "20";
                        if (Int16.Parse(new string(switchDOI)) > Int16.Parse(datenow.Substring(2, 2)))
                            year = "19";
                        var dateOfIssue = (Int16.Parse(year + new string(switchDOI)) + 543).ToString();

                        var licenseNumber = doc2.licenseNumber;//doc2.codeShort + doc2.codeShortNumber + "/" + dateOfIssue;
                        if (licenseNumber == value.licenseNumber)
                        {
                            var category = doc2.category == "ขั้นหนึ่ง" ? "1" : "2";
                            vetImageUrl = "http://vet.we-builds.com/vet-document/images/vet/" + category + "/" + doc2.codeShortNumber + ".jpg";
                            vetCategory = doc2.category;
                        }
                    }
                }


                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.code)) filter &= Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (!string.IsNullOrEmpty(value.profileCode)) filter &= Builders<BsonDocument>.Filter.Eq("code", value.profileCode);
                doc = col.Find(filter).FirstOrDefault();
                //var model = BsonSerializer.Deserialize<object>(doc);

                //if (value.category != "guest")
                //{
                //    doc["username"] = value.username;
                //}

                doc["imageUrl"] = value.imageUrl ?? "";
                doc["prefixName"] = value.prefixName ?? "";
                doc["firstName"] = value.firstName ?? "";
                doc["lastName"] = value.lastName ?? "";
                doc["birthDay"] = value.birthDay ?? "";
                doc["phone"] = value.phone ?? "";
                doc["email"] = value.email ?? "";
                //doc["facebookID"] = value.facebookID;
                //doc["googleID"] = value.googleID;
                //doc["lineID"] = value.lineID;
                //doc["line"] = value.line;
                //doc["password"] = value.password;
                doc["sex"] = value.sex ?? "";
                doc["soi"] = value.soi ?? "";
                doc["address"] = value.address ?? "";
                doc["moo"] = value.moo ?? "";
                doc["road"] = value.road ?? "";
                doc["tambonCode"] = value.tambonCode ?? "";
                doc["tambon"] = value.tambon ?? "";
                doc["amphoeCode"] = value.amphoeCode ?? "";
                doc["amphoe"] = value.amphoe ?? "";
                doc["provinceCode"] = value.provinceCode ?? "";
                doc["province"] = value.province ?? "";
                doc["postnoCode"] = value.postnoCode ?? "";
                doc["postno"] = value.postno ?? "";
                //doc["job"] = value.job;
                doc["idcard"] = value.idcard ?? "";
                //doc["officerCode"] = value.officerCode;
                doc["licenseNumber"] = value.licenseNumber;
                doc["countUnit"] = value.countUnit ?? "[]";
                doc["lv0"] = value.lv0 ?? "";
                doc["lv1"] = value.lv1 ?? "";
                doc["lv2"] = value.lv2 ?? "";
                doc["lv3"] = value.lv3 ?? "";
                doc["lv4"] = value.lv4 ?? "";
                //doc["linkAccount"] = value.linkAccount;
                //doc["description"] = value.description;
                //doc["isActive"] = value.isActive;
                doc["status"] = value.status ?? "";
                doc["updateBy"] = value.updateBy ?? "";
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["vetCategory"] = vetCategory ?? "";
                doc["vetImageUrl"] = vetImageUrl ?? "";
                col.ReplaceOne(filter, doc);

                var sv = new AES();

                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq(c => c.status, "D");
                if (!string.IsNullOrEmpty(value.code)) registerFilter &= Builders<Register>.Filter.Eq("code", value.code);
                if (!string.IsNullOrEmpty(value.profileCode)) registerFilter &= Builders<Register>.Filter.Eq("code", value.profileCode);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    password = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb"),
                    c.status,
                    c.isActive,
                    c.createBy,
                    c.createDate,
                    c.imageUrl,
                    c.updateBy,
                    c.updateDate,
                    c.createTime,
                    c.updateTime,
                    c.docDate,
                    c.docTime,
                    c.category,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    c.birthDay,
                    c.phone,
                    c.email,
                    c.facebookID,
                    c.googleID,
                    c.lineID,
                    //c.line,
                    c.sex,
                    c.soi,
                    c.address,
                    c.moo,
                    c.road,
                    c.tambon,
                    c.amphoe,
                    c.province,
                    c.postno,
                    c.tambonCode,
                    c.amphoeCode,
                    c.provinceCode,
                    c.postnoCode,
                    //c.job,
                    c.idcard,
                    c.officerCode,
                    c.licenseNumber,
                    c.countUnit,
                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,
                    c.linkAccount,
                    c.vetCategory,
                    c.vetImageUrl
                }).FirstOrDefault();

                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("apple/login")]
        public ActionResult<Response> AppleLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "apple");
                //filter &= Builders<Register>.Filter.Eq("appleID", value.appleID);

                if (!string.IsNullOrEmpty(value.email))
                    filter &= Builders<Register>.Filter.Eq("email", value.email);
                else
                    filter &= Builders<Register>.Filter.Eq("appleID", value.appleID);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "apple";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("facebook/login")]
        public ActionResult<Response> FacebookLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "facebook");

                if (!string.IsNullOrEmpty(value.email))
                    filter &= Builders<Register>.Filter.Eq("email", value.email);
                else
                    filter &= Builders<Register>.Filter.Eq("facebookID", value.facebookID);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "facebook";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("google/login")]
        public ActionResult<Response> GoogleLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "google");
                filter &= Builders<Register>.Filter.Eq("email", value.email);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "google";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("line/login")]
        public ActionResult<Response> LineLogin([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("category", "line");
                filter &= Builders<Register>.Filter.Eq("lineID", value.lineID);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();

                if (doc == null)
                {
                    // insert record and read again
                    value.category = "line";
                    this.create(value);
                    var newDoc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.prefixName, c.firstName, c.lastName, c.imageUrl, c.email, c.phone, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();
                    return new Response { status = "S", message = "success", objectData = newDoc };
                }
                else
                {
                    var update = Builders<Register>.Update.Set("imageUrl", value.imageUrl);
                    col.UpdateOne(filter, update);
                    doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName, c.countUnit, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv0List, c.lv1List, c.lv2List, c.lv3List, c.lv4List }).FirstOrDefault();

                    // read current record
                    return new Response { status = "S", message = "success", objectData = doc };
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // Function create
        private void create(Register value)
        {
            value.code = "".toCode();

            //insert record
            var newCol = new Database().MongoClient("register");

            //check duplicate
            var newFilter = Builders<BsonDocument>.Filter.Eq("code", value.code);
            if (newCol.Find(newFilter).Any())
                value.code = "".toCode();

            //return new Response { status = "E", message = $"code: {value.code} is exist", objectData = value };

            var newDoc = new BsonDocument
            {
                { "code", value.code },
                { "imageUrl", value.imageUrl },
                { "category", value.category },
                { "username", value.username },
                { "password", "" },
                { "prefixName", value.prefixName },
                { "firstName", value.firstName },
                { "lastName", value.lastName },
                { "email", value.email },
                { "appleID", value.appleID },
                { "facebookID", value.facebookID },
                { "googleID", value.googleID },
                { "lineID", value.lineID },
                { "countUnit", "[]" },
                { "lv0", "" },
                { "lv1", "" },
                { "lv2", "" },
                { "lv3", "" },
                { "lv4", "" },

                { "createBy", value.createBy },
                { "createDate", DateTime.Now.toStringFromDate() },
                { "createTime", DateTime.Now.toTimeStringFromDate() },
                { "updateBy", value.updateBy },
                { "updateDate", DateTime.Now.toStringFromDate() },
                { "updateTime", DateTime.Now.toTimeStringFromDate() },
                { "docDate", DateTime.Now.Date.AddHours(7) },
                { "docTime", DateTime.Now.toTimeStringFromDate() },
                { "isActive", true },
                { "status", "N" },
            };
            newCol.InsertOne(newDoc);

            //BEGIN : Statistic
            try
            {
                var value1 = new Criteria();
                var age = "";
                var countUnit = "";
                try
                {
                    var colCountUnit = new Database().MongoClient<Register>("organization");
                    countUnit = colCountUnit.Find(Builders<Register>.Filter.Eq("lv0", value.lv0)).Project(c => c.title).FirstOrDefault();

                }
                catch { }

                if (!string.IsNullOrEmpty(value.birthDay))
                {
                    //var year = value.birthDay.Substring(0, 4);
                    //var yearNow = DateTime.Now;
                    //age = year;
                }

                value1.title = value.category;
                value1.platform = value.platform;
                value1.countUnit = countUnit;
                value1.sex = value.sex;
                value1.age = age;
                value1.updateBy = value.username;

                if (!string.IsNullOrEmpty(value.code))
                    value1.reference = value.code;

                value1.statisticsCreateAsync("register");
            }
            catch { }
            //END : Statistic
        }

        // POST /organization/create
        [HttpPost("organization/create")]
        public ActionResult<Response> OrganizationCreate([FromBody] Organization value)
        {
            try
            {
                //get Organization
                var colOrganiztion = new Database().MongoClient<News>("organization");
                var filterOrganiztion = Builders<News>.Filter.Ne("status", "D");
                var docOraganiztion = colOrganiztion.Find(filterOrganiztion).Project(c => new { c.code, c.title }).ToList();

                //set model
                var model = new CountUnit
                {
                    lv0 = value.lv0,
                    titleLv0 = docOraganiztion.FirstOrDefault(c => c.code == value.lv0)?.title,
                    lv1 = value.lv1,
                    titleLv1 = docOraganiztion.FirstOrDefault(c => c.code == value.lv1)?.title,
                    lv2 = value.lv2,
                    titleLv2 = docOraganiztion.FirstOrDefault(c => c.code == value.lv2)?.title,
                    lv3 = value.lv3,
                    titleLv3 = docOraganiztion.FirstOrDefault(c => c.code == value.lv3)?.title,
                    lv4 = value.lv4,
                    titleLv4 = docOraganiztion.FirstOrDefault(c => c.code == value.lv4)?.title,
                    status = "A"
                };

                //update countUnit
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq("code", value.profileCode);
                var doc = col.Find(filter).Project(c => c.countUnit).FirstOrDefault();
                var objject = JsonConvert.DeserializeObject<List<CountUnit>>(doc);
                objject.Add(model);
                var jsonString = JsonConvert.SerializeObject(objject);

                var update = Builders<Register>.Update.Set("countUnit", jsonString).Set("status","A");
                col.UpdateOne(filter, update);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /organization/read
        [HttpPost("organization/read")]
        public ActionResult<Response> OrganizationRead([FromBody] Criteria value)
        {
            try
            {
                //update getdata CountUnit
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq("code", value.profileCode);
                var doc = col.Find(filter).Project(c => c.countUnit).FirstOrDefault();
                var model = JsonConvert.DeserializeObject<List<CountUnit>>(doc);

                //by values CountUnit
                var listdata = new List<object>();
                model.Where(c => c.status != "D").ToList().ForEach(f => { listdata.Add(new
                {
                    lv0 = (f.lv0 ?? ""),
                    lv1 = (f.lv1 ?? ""),
                    lv2 = (f.lv2 ?? ""),
                    lv3 = (f.lv3 ?? ""),
                    lv4 = (f.lv4 ?? ""),
                    lv5 = (f.lv5 ?? ""),
                    titleLv0 = (f.titleLv0 ?? ""),
                    titleLv1 = (f.titleLv1 ?? ""),
                    titleLv2 = (f.titleLv2 ?? ""),
                    titleLv3 = (f.titleLv3 ?? ""),
                    titleLv4 = (f.titleLv4 ?? ""),
                    titleLv5 = (f.titleLv5 ?? ""),
                    status = (f.status == "A" ? "  อนุมัติใช้งาน"
                            : f.status == "V" || f.status == "R" ? "  กำลังดำเนินการ"
                            : f.status == "N" ? "  ไม่ผ่านการอนุมัติ"
                            : ""),
                    colorId = f.status == "A" ? 0xFF5AAC68
                            : f.status == "V" || f.status == "R" ? 0xFFFF7900
                            : f.status == "N" ? 0xFF707070
                            : 0xFFFFFFFF,
                    statusCode = f.status
                });
                });

                return new Response { status = "S", message = "success", objectData = listdata };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /organization/delete
        [HttpPost("organization/delete")]
        public ActionResult<Response> OrganizationDelete([FromBody] Organization value)
        {
            try
            {
                //update countUnit
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq("code", value.profileCode);
                var doc = col.Find(filter).Project(c => c.countUnit).FirstOrDefault();
                var objject = JsonConvert.DeserializeObject<List<CountUnit>>(doc);
                objject.Where(c => (c.lv0 ?? "") == (value.lv0 ?? "")
                                                              && (c.lv1 ?? "") == (value.lv1 ?? "")
                                                              && (c.lv2 ?? "") == (value.lv2 ?? "")
                                                              && (c.lv3 ?? "") == (value.lv3 ?? "")
                                                              && (c.lv4 ?? "") == (value.lv4 ?? "")
                                                              ).ToList().ForEach(f => f.status = "D");
                var jsonString = JsonConvert.SerializeObject(objject);

                if (objject.Where(c => c.status == "A").Count() == 0)
                {
                    var update = Builders<Register>.Update.Set("countUnit", jsonString).Set("status", "N");
                    col.UpdateOne(filter, update);
                }
                else
                {
                    var update = Builders<Register>.Update.Set("countUnit", jsonString);
                    col.UpdateOne(filter, update);
                }

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("token/create")]
        public ActionResult<Response> TokenCreate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                {
                    var doc2 = new BsonDocument();
                    var col2 = new Database().MongoClient("register");
                    var filter2 = Builders<BsonDocument>.Filter.Eq("code", value.profileCode);
                    doc2 = col2.Find(filter2).FirstOrDefault();
                    doc2["token"] = value.token;
                    col2.ReplaceOne(filter2, doc2);
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

    }
}