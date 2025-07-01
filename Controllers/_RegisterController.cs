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
    public class RegisterController : Controller
    {
        public RegisterController() { }

        #region login

        // POST /login
        [HttpPost("login")]
        public ActionResult<Response> Login([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true) & Builders<Register>.Filter.Ne(x => x.status, "D");
                filter &= Builders<Register>.Filter.Eq("username", value.username);

                var sv = new AES();
                var encryptPassword = "";
                if (!string.IsNullOrEmpty(value.password))
                    encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                filter &= Builders<Register>.Filter.Eq("password", encryptPassword);

                var doc = col.Find(filter).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.center }).FirstOrDefault();

                if (doc != null)
                {
                    value.code = value.code.toCode();
                    var token = $"{doc.username}|{doc.password}|{doc.category}|{value.code}".toEncode();

                    //BEGIN =disable session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colSession = new Database().MongoClient<Register>("registerSession");
                        var filterSession = Builders<Register>.Filter.Eq("username", value.username);
                        filterSession = filterSession & Builders<Register>.Filter.Eq("isActive", true);

                        //get last session
                        var docSession = colSession.Find(filterSession).Project(c => new { c.token }).FirstOrDefault();

                        //update last session
                        var updateSession = Builders<Register>.Update.Set("isActive", false).Set("updateBy", "system").Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                        colSession.UpdateMany(filterSession, updateSession);

                        //set activity
                        if (docSession != null)
                        {
                            {
                                var colActivity = new Database().MongoClient<Register>("registerActivity");

                                //update last activity
                                var updateActivity = Builders<Register>.Update.Set("isActive", false).Set("updateBy", "system").Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                                colActivity.UpdateMany(filterSession, updateActivity);

                            }

                            {
                                var colActivity = new Database().MongoClient("registerActivity");

                                var docActivity = new BsonDocument
                                {
                                    { "token", docSession.token },
                                    { "code", value.code },
                                    { "username", value.username },
                                    { "description", "ออกจากระบบเนื่องจากมีการเข้าใช้งานจากที่อื่น" },
                                    { "createBy", "system" },
                                    { "createDate", DateTime.Now.toStringFromDate() },
                                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                                    { "updateBy", "system" },
                                    { "updateDate", DateTime.Now.toStringFromDate() },
                                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                                    { "docDate", DateTime.Now.Date.AddHours(7) },
                                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                                    { "isActive", false }
                                };
                                colActivity.InsertOne(docActivity);
                            }

                        }
                    }

                    //END =disable seesion <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                    //BEGIN =create session >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colSession = new Database().MongoClient("registerSession");
                        var docSession = new BsonDocument
                        {
                            { "token", token },
                            { "code", value.code },
                            { "username", value.username },
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
                        colSession.InsertOne(docSession);
                    }

                    //END =create session <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<





                    //BEGIN =create activity >>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    {
                        var colActivity = new Database().MongoClient("registerActivity");
                        var docActivity = new BsonDocument
                        {
                            { "token", token },
                            { "code", value.code },
                            { "username", value.username },
                            { "description", "เข้าใช้งานระบบ" },
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
                        colActivity.InsertOne(docActivity);
                    }

                    //END =create activity <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                    return new Response { status = "S", message = "success", jsonData = token, objectData = new { code = doc.code, username = value.username, category = doc.category, imageUrl = doc.imageUrl ,center = doc.center} };
                }
                else
                {
                    return new Response { status = "F", message = "login failed", jsonData = "", objectData = "" };
                }

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("register");
                var colCenter = new Database().MongoClient("mCenter");

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("username", value.username) & Builders<BsonDocument>.Filter.Ne("status", "D");
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"username= {value.username} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                //{
                //    //check duplicate
                //    var filter = Builders<BsonDocument>.Filter.Eq("idcard", value.idcard) & Builders<BsonDocument>.Filter.Ne("status", "D");
                //    if (col.Find(filter).Any())
                //    {
                //        return new Response { status = "E", message = $"idcard= {value.idcard} is exist", jsonData = value.ToJson(), objectData = value };
                //    }
                //}

                var sv = new AES();
                var encryptPassword = "";
                if (!string.IsNullOrEmpty(value.password))
                    value.password = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "username", value.username },
                    { "password", value.password },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "birthDay", value.birthDay },
                    { "phone", value.phone },
                    { "email", value.email },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "status", "A" },
                };
                col.InsertOne(doc);

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
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne("status", "D")
                    & (Builders<Register>.Filter.Ne(x => x.category, "guest")) & (Builders<Register>.Filter.Ne(x => x.category, "facebook")) & (Builders<Register>.Filter.Ne(x => x.category, "google")) & (Builders<Register>.Filter.Ne(x => x.category, "line") & (Builders<Register>.Filter.Ne(x => x.category, "apple")));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }
                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Regex("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).ToList();

                docs.ForEach(c =>
                {
                    var sv = new AES();
                    var encryptPassword = "";
                    if (!string.IsNullOrEmpty(c.password))
                        encryptPassword = sv.AesDecryptECB(c.password, "p3s6v8y/B?E(H+Mb");

                    c.password = encryptPassword;
                });

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
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
                var col = new Database().MongoClient( "register");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.password)) {

                    var sv = new AES();
                    var encryptPassword = "";
                    if (!string.IsNullOrEmpty(value.password))
                        encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                    doc["password"] = encryptPassword;

                }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.prefixName)) { doc["prefixName"] = value.prefixName; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["firstName"] = value.firstName; }
                if (!string.IsNullOrEmpty(value.lastName)) { doc["lastName"] = value.lastName; }
                if (!string.IsNullOrEmpty(value.phone)) { doc["phone"] = value.phone; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                if (!string.IsNullOrEmpty(value.birthDay)) { doc["birthDay"] = value.birthDay; }

                doc["center"] = value.center;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient( "register");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code= {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region category

        // POST /create
        [HttpPost("category/create")]
        public ActionResult<Response> CategoryCreate([FromBody] RegisterCategory value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerCategory");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "createAction", value.createAction },
                    { "readAction", value.readAction },
                    { "updateAction", value.updateAction },
                    { "deleteAction", value.deleteAction },
                    { "approveAction", value.approveAction },

                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },

                    { "organizationPage", value.organizationPage },
                    { "userRolePage", value.userRolePage },
                    { "memberPage", value.memberPage },
                    { "memberMobilePage", value.memberMobilePage },
                    { "personnelPage", value.personnelPage },
                    { "personnelStructurePage", value.personnelStructurePage },

                    { "logoPage", value.logoPage },
                    { "splashPage", value.splashPage },
                    { "mainPopupPage", value.mainPopupPage },
                    { "bannerPage", value.bannerPage },
                    { "forceAdsPage", value.forceAdsPage },
                    { "rotationPage", value.rotationPage },
                    { "partnerPage", value.partnerPage },
                    { "alliancePage", value.alliancePage },
                    { "relateAgencyPage", value.relateAgencyPage},
                    { "officeActivitiesPage", value.officeActivitiesPage },

                    { "newsPage", value.newsPage },
                    { "importantPage", value.importantPage },
                    { "eventPage", value.eventPage },
                    { "contactPage", value.contactPage },
                    { "knowledgePage", value.knowledgePage },
                    { "knowledgeVetPage", value.knowledgeVetPage },
                    { "privilegePage", value.privilegePage },
                    { "poiPage", value.poiPage },
                    { "pollPage", value.pollPage },
                    { "suggestionPage", value.suggestionPage },
                    { "notificationPage", value.notificationPage },
                    { "welfarePage", value.welfarePage },
                    { "trainingPage", value.trainingPage },
                    { "reporterPage", value.reporterPage },
                    { "warningPage", value.warningPage },
                    { "fundPage", value.fundPage },
                    { "cooperativeFormPage", value.cooperativeFormPage },
                    { "examinationPage", value.examinationPage },
                    { "imageEventPage", value.imageEventPage },
                    { "eventAbroadPage", value.eventAbroadPage },
                    { "vetEnewsPage", value.vetEnewsPage },
                    { "lawPage", value.lawPage },
                    { "expertBranchPage", value.expertBranchPage },
                    { "verifyApprovedUserPage", value.verifyApprovedUserPage },
                    { "trainingInstitutePage", value.trainingInstitutePage },
                    { "seminarPage", value.seminarPage },

                    { "productPage", value.productPage },
                    { "employeePage", value.employeePage },
                    { "workProcessPage", value.workProcessPage },
                    { "portfolioPage", value.portfolioPage },
                    { "certificatePage", value.certificatePage },

                    { "productCategoryPage", value.productCategoryPage },
                    { "portfolioCategoryPage", value.portfolioCategoryPage },
                    { "employeeCategoryPage", value.employeeCategoryPage },
                    { "certificateCategoryPage", value.certificateCategoryPage },


                    { "newsCategoryPage", value.newsCategoryPage },
                    { "importantCategoryPage", value.importantCategoryPage },
                    { "eventCategoryPage", value.eventCategoryPage },
                    { "contactCategoryPage", value.contactCategoryPage },
                    { "knowledgeCategoryPage", value.knowledgeCategoryPage },
                    { "knowledgeVetCategoryPage", value.knowledgeVetCategoryPage },
                    { "privilegeCategoryPage", value.privilegeCategoryPage },
                    { "poiCategoryPage", value.poiCategoryPage },
                    { "pollCategoryPage", value.pollCategoryPage },
                    { "suggestionCategoryPage", value.suggestionCategoryPage },
                    { "notificationCategoryPage", value.notificationCategoryPage },
                    { "welfareCategoryPage", value.welfareCategoryPage },
                    { "trainingCategoryPage", value.trainingCategoryPage },
                    { "reporterCategoryPage", value.reporterCategoryPage },
                    { "warningCategoryPage", value.warningCategoryPage },
                    { "fundCategoryPage", value.fundCategoryPage },
                    { "cooperativeFormCategoryPage", value.cooperativeFormCategoryPage },
                    { "examinationCategoryPage", value.examinationCategoryPage },
                    { "imageEventCategoryPage", value.imageEventCategoryPage },
                    { "eventAbroadCategoryPage", value.eventAbroadCategoryPage },
                    { "vetEnewsCategoryPage", value.vetEnewsCategoryPage },
                    { "websitevisitorPage", value.websitevisitorPage },
                    { "cmsvisitorPage", value.cmsvisitorPage },
                    { "aboutCommentPage", value.aboutCommentPage },
                    { "personnelStructureCategoryPage", value.personnelStructureCategoryPage },
                    { "personnelStructureCategoryPage2", value.personnelStructureCategoryPage2 },
                    { "lawCategoryPage", value.lawCategoryPage },
                    { "expertBranchCategoryPage", value.expertBranchCategoryPage },
                    { "verifyApprovedUserCategoryPage", value.verifyApprovedUserCategoryPage },
                    { "trainingInstituteCategoryPage", value.trainingInstituteCategoryPage },
                    { "seminarCategoryPage", value.seminarCategoryPage },

                    { "policyApplicationPage", value.policyApplicationPage },
                    { "policyMarketingPage", value.policyMarketingPage },
                    { "memberMobilePolicyApplicationPage", value.memberMobilePolicyApplicationPage },
                    { "memberMobilePolicyMarketingPage", value.memberMobilePolicyMarketingPage },
                    //report
                    { "reportNumberMemberRegisterPage", value.reportNumberMemberRegisterPage },
                    { "reportMemberRegisterPage", value.reportMemberRegisterPage },
                    { "reportNewsCategoryPage", value.reportNewsCategoryPage },
                    { "reportNewsPage", value.reportNewsPage },
                    { "reportKnowledgeCategoryPage", value.reportKnowledgeCategoryPage },
                    { "reportKnowledgePage", value.reportKnowledgePage },
                    { "reportReporterPage", value.reportReporterPage },
                    { "reportContactPage", value.reportContactPage },
                    { "reportEventCalendarPage", value.reportEventCalendarPage },
                    { "reportPoiPage", value.reportPoiPage },
                    { "reportPollPage", value.reportPollPage },
                    { "reportReporterCreatePage", value.reportReporterCreatePage },
                    { "reportWarningPage", value.reportWarningPage },
                    { "reportWelfarePage", value.reportWelfarePage },
                    { "reportPrivilegePage", value.reportPrivilegePage },

                    { "reportNewsKeysearchPage", value.reportNewsKeysearchPage },
                    { "reportKnowledgeKeysearchPage", value.reportKnowledgeKeysearchPage },
                    { "reportEventCalendarKeysearchPage", value.reportEventCalendarKeysearchPage },
                    { "reportContactKeysearchPage", value.reportContactKeysearchPage },
                    { "reportPrivilegeKeysearchPage", value.reportPrivilegeKeysearchPage },
                    { "reportPoiKeysearchPage", value.reportPoiKeysearchPage },
                    { "reportPollKeysearchPage", value.reportPollKeysearchPage },
                    { "reportReporterKeysearchPage", value.reportReporterKeysearchPage },
                    { "reportWarningKeysearchPage", value.reportWarningKeysearchPage },
                    { "reportWelfareKeysearchPage", value.reportWelfareKeysearchPage },

                    { "reportAboutUsPage", value.reportAboutUsPage },
                    { "reportRotationPage", value.reportRotationPage },
                    { "reportForceAdsPage", value.reportForceAdsPage },
                    //Master
                    { "swearWordsPage", value.swearWordsPage },
                    { "masterVeterinaryPage", value.masterVeterinaryPage },
                    { "contentKeywordPage", value.contentKeywordPage },
                    { "notificationResultPage", value.notificationResultPage },
                    { "notificationExamPage", value.notificationExamPage },
                    { "dashboardPage", value.dashboardPage },

                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "status", "A" }
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
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<RegisterCategory>( "registerCategory");

                var filter = Builders<RegisterCategory>.Filter.Ne(x => x.status, "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<RegisterCategory>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<RegisterCategory>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                    //filter = (filter & Builders<RegisterCategory>.Filter.Regex("title", value.keySearch)) | (filter & Builders<RegisterCategory>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<RegisterCategory>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<RegisterCategory>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    //if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<RegisterCategory>.Filter.Regex("description", value.description); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", ds.start) & Builders<RegisterCategory>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", ds.start) & Builders<RegisterCategory>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<RegisterCategory>.Filter.Gt("docDate", de.start) & Builders<RegisterCategory>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new
                {
                    c.code,
                    c.title,
                    c.createBy,
                    c.createDate,
                    c.isActive,
                    c.updateDate,
                    c.updateBy,

                    c.createAction,
                    c.readAction,
                    c.updateAction,
                    c.deleteAction,
                    c.approveAction,

                    c.lv0,
                    c.lv1,
                    c.lv2,
                    c.lv3,
                    c.lv4,

                    c.organizationPage,
                    c.userRolePage,
                    c.memberPage,
                    c.memberMobilePage,
                    c.personnelPage,
                    c.personnelStructurePage,

                    c.logoPage,
                    c.splashPage,
                    c.mainPopupPage,
                    c.bannerPage,
                    c.forceAdsPage,
                    c.rotationPage,
                    c.partnerPage,
                    c.alliancePage,
                    c.relateAgencyPage,
                    c.officeActivitiesPage,

                    c.newsPage,
                    c.importantPage,
                    c.eventPage,
                    c.contactPage,
                    c.knowledgePage,
                    c.knowledgeVetPage,
                    c.privilegePage,
                    c.poiPage,
                    c.pollPage,
                    c.suggestionPage,
                    c.notificationPage,
                    c.welfarePage,
                    c.trainingPage,
                    c.reporterPage,
                    c.warningPage,
                    c.fundPage,
                    c.cooperativeFormPage,
                    c.examinationPage,
                    c.imageEventPage,
                    c.eventAbroadPage,
                    c.vetEnewsPage,
                    c.policyApplicationPage,
                    c.policyMarketingPage,
                    c.memberMobilePolicyApplicationPage,
                    c.memberMobilePolicyMarketingPage,
                    c.lawPage,
                    c.expertBranchPage,
                    c.verifyApprovedUserPage,
                    c.trainingInstitutePage,
                    c.seminarPage,

                    c.productPage,
                    c.productCategoryPage,
                    c.portfolioCategoryPage,

                    c.employeePage,
                    c.employeeCategoryPage,
                    c.portfolioPage,
                    c.workProcessPage,
                    c.certificatePage,
                    c.certificateCategoryPage,

                    c.newsCategoryPage,
                    c.importantCategoryPage,
                    c.eventCategoryPage,
                    c.contactCategoryPage,
                    c.knowledgeCategoryPage,
                    c.knowledgeVetCategoryPage,
                    c.privilegeCategoryPage,
                    c.poiCategoryPage,
                    c.pollCategoryPage,
                    c.suggestionCategoryPage,
                    c.notificationCategoryPage,
                    c.welfareCategoryPage,
                    c.trainingCategoryPage,
                    c.reporterCategoryPage,
                    c.warningCategoryPage,
                    c.fundCategoryPage,
                    c.cooperativeFormCategoryPage,
                    c.examinationCategoryPage,
                    c.imageEventCategoryPage,
                    c.eventAbroadCategoryPage,
                    c.vetEnewsCategoryPage,
                    c.websitevisitorPage,
                    c.cmsvisitorPage,
                    c.aboutCommentPage,
                    c.personnelStructureCategoryPage,
                    c.personnelStructureCategoryPage2,
                    c.lawCategoryPage,
                    c.expertBranchCategoryPage,
                    c.verifyApprovedUserCategoryPage,
                    c.trainingInstituteCategoryPage,
                    c.seminarCategoryPage,
                    //report
                    c.reportNumberMemberRegisterPage,
                    c.reportMemberRegisterPage,
                    c.reportNewsCategoryPage,
                    c.reportNewsPage,
                    c.reportKnowledgeCategoryPage,
                    c.reportKnowledgePage,

                    c.reportReporterPage,
                    c.reportContactPage,
                    c.reportEventCalendarPage,
                    c.reportPrivilegePage,
                    c.reportPoiPage,
                    c.reportPollPage,
                    c.reportReporterCreatePage,
                    c.reportWarningPage,
                    c.reportWelfarePage,

                    c.reportNewsKeysearchPage,
                    c.reportKnowledgeKeysearchPage,
                    c.reportEventCalendarKeysearchPage,
                    c.reportContactKeysearchPage,
                    c.reportPrivilegeKeysearchPage,
                    c.reportPoiKeysearchPage,
                    c.reportPollKeysearchPage,
                    c.reportReporterKeysearchPage,
                    c.reportWarningKeysearchPage,
                    c.reportWelfareKeysearchPage,

                    c.reportAboutUsPage,
                    c.reportForceAdsPage,
                    c.reportRotationPage,
                    //Master
                    c.swearWordsPage,
                    c.masterVeterinaryPage,
                    c.contentKeywordPage,
                    c.notificationResultPage,
                    c.notificationExamPage,
                    c.dashboardPage,
                }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> CategoryUpdate([FromBody] RegisterCategory value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }

                doc["createAction"] = value.createAction;
                doc["readAction"] = value.readAction;
                doc["updateAction"] = value.updateAction;
                doc["deleteAction"] = value.deleteAction;
                doc["approveAction"] = value.approveAction;

                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;

                doc["organizationPage"] = value.organizationPage;
                doc["userRolePage"] = value.userRolePage;
                doc["memberPage"] = value.memberPage;
                doc["memberMobilePage"] = value.memberMobilePage;
                doc["personnelPage"] = value.personnelPage;
                doc["personnelStructurePage"] = value.personnelStructurePage;

                doc["logoPage"] = value.logoPage;
                doc["splashPage"] = value.splashPage;
                doc["mainPopupPage"] = value.mainPopupPage;
                doc["bannerPage"] = value.bannerPage;
                doc["forceAdsPage"] = value.forceAdsPage;
                doc["rotationPage"] = value.rotationPage;
                doc["partnerPage"] = value.partnerPage;
                doc["alliancePage"] = value.alliancePage;
                doc["relateAgencyPage"] = value.relateAgencyPage;
                doc["officeActivitiesPage"] = value.officeActivitiesPage;

                doc["newsPage"] = value.newsPage;
                doc["importantPage"] = value.importantPage;
                doc["eventPage"] = value.eventPage;
                doc["contactPage"] = value.contactPage;
                doc["knowledgePage"] = value.knowledgePage;
                doc["knowledgeVetPage"] = value.knowledgeVetPage;
                doc["privilegePage"] = value.privilegePage;
                doc["poiPage"] = value.poiPage;
                doc["pollPage"] = value.pollPage;
                doc["suggestionPage"] = value.suggestionPage;
                doc["notificationPage"] = value.notificationPage;
                doc["welfarePage"] = value.welfarePage;
                doc["trainingPage"] = value.trainingPage;
                doc["reporterPage"] = value.reporterPage; 
                doc["warningPage"] = value.warningPage;
                doc["fundPage"] = value.fundPage;
                doc["cooperativeFormPage"] = value.cooperativeFormPage;
                doc["examinationPage"] = value.examinationPage;
                doc["imageEventPage"] = value.imageEventPage;
                doc["eventAbroadPage"] = value.eventAbroadPage;
                doc["vetEnewsPage"] = value.vetEnewsPage;
                doc["lawPage"] = value.lawPage;
                doc["expertBranchPage"] = value.expertBranchPage;
                doc["verifyApprovedUserPage"] = value.verifyApprovedUserPage;
                doc["trainingInstitutePage"] = value.trainingInstitutePage;
                doc["seminarPage"] = value.seminarPage;

                doc["productPage"] = value.productPage;
                doc["employeePage"] = value.employeePage;
                doc["workProcessPage"] = value.workProcessPage;
                doc["portfolioPage"] = value.portfolioPage;
                doc["certificatePage"] = value.certificatePage;

                doc["productCategoryPage"] = value.productCategoryPage;
                doc["portfolioCategoryPage"] = value.portfolioCategoryPage;
                doc["employeeCategoryPage"] = value.employeeCategoryPage;
                doc["certificateCategoryPage"] = value.certificateCategoryPage;

                doc["policyApplicationPage"] = value.policyApplicationPage;
                doc["policyMarketingPage"] = value.policyMarketingPage;
                doc["memberMobilePolicyApplicationPage"] = value.memberMobilePolicyApplicationPage;
                doc["memberMobilePolicyMarketingPage"] = value.memberMobilePolicyMarketingPage;

                doc["newsCategoryPage"] = value.newsCategoryPage;
                doc["importantCategoryPage"] = value.importantCategoryPage;
                doc["eventCategoryPage"] = value.eventCategoryPage;
                doc["contactCategoryPage"] = value.contactCategoryPage;
                doc["knowledgeCategoryPage"] = value.knowledgeCategoryPage;
                doc["knowledgeVetCategoryPage"] = value.knowledgeVetCategoryPage;
                doc["privilegeCategoryPage"] = value.privilegeCategoryPage;
                doc["poiCategoryPage"] = value.poiCategoryPage;
                doc["pollCategoryPage"] = value.pollCategoryPage;
                doc["suggestionCategoryPage"] = value.suggestionCategoryPage;
                doc["notificationCategoryPage"] = value.notificationCategoryPage;
                doc["welfareCategoryPage"] = value.welfareCategoryPage;
                doc["trainingCategoryPage"] = value.trainingCategoryPage;
                doc["reporterCategoryPage"] = value.reporterCategoryPage;
                doc["warningCategoryPage"] = value.warningCategoryPage;
                doc["fundCategoryPage"] = value.fundCategoryPage;
                doc["cooperativeFormCategoryPage"] = value.cooperativeFormCategoryPage;
                doc["examinationCategoryPage"] = value.examinationCategoryPage;
                doc["imageEventCategoryPage"] = value.imageEventCategoryPage;
                doc["eventAbroadCategoryPage"] = value.eventAbroadCategoryPage;
                doc["vetEnewsCategoryPage"] = value.vetEnewsCategoryPage;
                doc["websitevisitorPage"] = value.websitevisitorPage;
                doc["cmsvisitorPage"] = value.cmsvisitorPage;
                doc["aboutCommentPage"] = value.aboutCommentPage;
                doc["personnelStructureCategoryPage"] = value.personnelStructureCategoryPage;
                doc["personnelStructureCategoryPage2"] = value.personnelStructureCategoryPage2;
                doc["lawCategoryPage"] = value.lawCategoryPage;
                doc["expertBranchCategoryPage"] = value.expertBranchCategoryPage;
                doc["verifyApprovedUserCategoryPage"] = value.verifyApprovedUserCategoryPage;
                doc["trainingInstituteCategoryPage"] = value.trainingInstituteCategoryPage;
                doc["seminarCategoryPage"] = value.seminarCategoryPage;

                //report
                doc["reportNumberMemberRegisterPage"] = value.reportNumberMemberRegisterPage;
                doc["reportMemberRegisterPage"] = value.reportMemberRegisterPage;
                doc["reportNewsCategoryPage"] = value.reportNewsCategoryPage;
                doc["reportNewsPage"] = value.reportNewsPage;
                doc["reportKnowledgeCategoryPage"] = value.reportKnowledgeCategoryPage;
                doc["reportKnowledgePage"] = value.reportKnowledgePage;
                doc["reportReporterPage"] = value.reportReporterPage;
                doc["reportContactPage"] = value.reportContactPage;
                doc["reportEventCalendarPage"] = value.reportEventCalendarPage;
                doc["reportPrivilegePage"] = value.reportPrivilegePage;
                doc["reportPoiPage"] = value.reportPoiPage;
                doc["reportPollPage"] = value.reportPollPage;
                doc["reportReporterCreatePage"] = value.reportReporterCreatePage;
                doc["reportWarningPage"] = value.reportWarningPage;
                doc["reportWelfarePage"] = value.reportWelfarePage;

                doc["reportNewsKeysearchPage"] = value.reportNewsKeysearchPage;
                doc["reportKnowledgeKeysearchPage"] = value.reportKnowledgeKeysearchPage;
                doc["reportEventCalendarKeysearchPage"] = value.reportEventCalendarKeysearchPage;
                doc["reportContactKeysearchPage"] = value.reportContactKeysearchPage;
                doc["reportPrivilegeKeysearchPage"] = value.reportPrivilegeKeysearchPage;
                doc["reportPoiKeysearchPage"] = value.reportPoiKeysearchPage;
                doc["reportPollKeysearchPage"] = value.reportPollKeysearchPage;
                doc["reportReporterKeysearchPage"] = value.reportReporterKeysearchPage;
                doc["reportWarningKeysearchPage"] = value.reportWarningKeysearchPage;
                doc["reportWelfareKeysearchPage"] = value.reportWelfareKeysearchPage;

                doc["reportAboutUsPage"] = value.reportAboutUsPage;
                doc["reportForceAdsPage"] = value.reportForceAdsPage;
                doc["reportRotationPage"] = value.reportRotationPage;
                //Master
                doc["swearWordsPage"] = value.swearWordsPage;
                doc["masterVeterinaryPage"] = value.masterVeterinaryPage;
                doc["contentKeywordPage"] = value.contentKeywordPage;
                doc["notificationResultPage"] = value.notificationResultPage;
                doc["notificationExamPage"] = value.notificationExamPage;
                doc["dashboardPage"] = value.dashboardPage;

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = true;
                doc["status"] = "A";
                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("category/delete")]
        public ActionResult<Response> CategoryDelete([FromBody] Category value)
        {
            try
            {
                var col = new Database().MongoClient( "registerCategory");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate().Set("updateTime", DateTime.Now.toTimeStringFromDate());
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code= {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region permission (item ของ category)

        // POST /create
        [HttpPost("permission/create")]
        public ActionResult<Response> PermissionCreate([FromBody] Permission value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerPermission");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "page", value.page },
                    { "category", value.category },
                    { "reference", value.reference },

                    { "newsPage", value.newsPage },
                    { "importantPage", value.importantPage },
                    { "eventPage", value.eventPage },
                    { "contactPage", value.contactPage },
                    { "knowledgePage", value.knowledgePage },
                    { "knowledgeVetPage", value.knowledgeVetPage },
                    { "privilegePage", value.privilegePage },
                    { "poiPage", value.poiPage },
                    { "pollPage", value.pollPage },
                    { "suggestionPage", value.suggestionPage },
                    { "notificationPage", value.notificationPage },
                    { "welfarePage", value.welfarePage },
                    { "trainingPage", value.trainingPage },
                    { "reporterPage", value.reporterPage },
                    { "warningPage", value.warningPage },
                    { "fundPage", value.fundPage },
                    { "cooperativeFormPage", value.cooperativeFormPage },
                    { "examinationPage", value.examinationPage },
                    { "imageEventPage", value.imageEventPage },
                    { "eventAbroadPage", value.eventAbroadPage },
                    { "vetEnewsPage", value.vetEnewsPage},
                    { "seminarPage", value.seminarPage},

                    { "productPage", value.productPage},
                    { "employeePage", value.employeePage},
                    { "workProcessPage", value.workProcessPage},
                    { "portfolioPage", value.portfolioPage},
                    { "certificatePage", value.certificatePage},


                    { "lawPage", value.lawPage},
                    { "expertBranchPage", value.expertBranchPage},
                    { "verifyApprovedUserPage", value.verifyApprovedUserPage},
                    { "trainingInstitutePage", value.trainingInstitutePage},
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

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("permission/read")]
        public ActionResult<Response> PermissionRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Permission>( "registerPermission");

                var filter = Builders<Permission>.Filter.Eq(x => x.isActive, true);

                if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Permission>.Filter.Eq("reference", value.code); }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.page, c.category, c.newsPage, c.eventPage, c.contactPage, c.knowledgePage, c.privilegePage, c.poiPage, c.pollPage, c.suggestionPage, c.notificationPage }).ToList();
                List<Permission> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit)
                                     .Lookup("newsCategory", "category", "code", "newsCategoryList")
                                     .Lookup("importantCategory", "category", "code", "importantCategoryList")
                                     .Lookup("eventCalendarCategory", "category", "code", "eventCategoryList")
                                     .Lookup("contactCategory", "category", "code", "contactCategoryList")
                                     .Lookup("knowledgeCategory", "category", "code", "knowledgeCategoryList")
                                     .Lookup("privilegeCategory", "category", "code", "privilegeCategoryList")
                                     .Lookup("poiCategory", "category", "code", "poiCategoryList")
                                     .Lookup("pollCategory", "category", "code", "pollCategoryList")
                                     .Lookup("suggestionCategory", "category", "code", "suggestionCategoryList")
                                     .Lookup("notificationCategory", "category", "code", "notificationCategoryList")
                                     .Lookup("welfareCategory", "category", "code", "welfareCategoryList")
                                     .Lookup("trainingCategory", "category", "code", "trainingCategoryList")
                                     .Lookup("reporterCategory", "category", "code", "reporterCategoryList")
                                     .Lookup("warningCategory", "category", "code", "warningCategoryList")
                                     .Lookup("fundCategory", "category", "code", "fundCategoryList")
                                     .Lookup("cooperativeFormCategory", "category", "code", "cooperativeFormCategoryList")
                                     .Lookup("examinationCategory", "category", "code", "examinationCategoryList")
                                     .Lookup("imageEventCategory", "category", "code", "imageEventCategoryList")
                                     .Lookup("eventAbroadCategory", "category", "code", "eventAbroadCategoryList")
                                     .Lookup("vetEnewsCategory", "category", "code", "vetEnewsCategoryList")
                                     .Lookup("knowledgeVetCategory", "category", "code", "knowledgeVetCategoryList")
                                     .Lookup("lawCategory", "category", "code", "lawCategoryList")
                                     .Lookup("expertBranchCategory", "category", "code", "expertBranchCategoryList")
                                     .Lookup("verifyApprovedUserCategory", "category", "code", "verifyApprovedUserCategoryList")
                                     .Lookup("trainingInstituteCategory", "category", "code", "trainingInstituteCategoryList")
                                     .Lookup("seminarCategory", "category", "code", "seminarCategoryList")
                                     .Lookup("productCategory", "category", "code", "productCategoryList")
                                     .Lookup("employeeCategory", "category", "code", "employeeCategoryList")
                                     .Lookup("certificateCategory", "category", "code", "certificateCategoryList")
                                     .As<Permission>()
                                     .ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /create
        [HttpPost("permission/delete")]
        public ActionResult<Response> PermissionDelete([FromBody] Permission value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("registerPermission");

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

        #region role (register + category)

        // POST /create
        [HttpPost("role/create")]
        public ActionResult<Response> RoleCreate([FromBody] Register value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("registerRole");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "username", value.username },
                    { "category", value.category },

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

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("role/read")]
        public ActionResult<Response> RoleRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>( "registerRole");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);

                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.username, c.category }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /create
        [HttpPost("role/delete")]
        public ActionResult<Response> RoleDelete([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "registerRole");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("username", value.username);
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

        #region authentication

        // POST /create
        [HttpPost("menu/create")]
        public ActionResult<Response> menuCreate([FromBody] Register value)
        {
            value.code = value.code.toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("mMenu");

                {
                    //check duplicate
                    var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    if (col.Find(filter).Any())
                    {
                        return new Response { status = "E", message = $"code= {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                    }
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "category", value.category },
                    { "sequence", value.sequence },
                    { "title", value.title },
                    { "routing", value.codeShort },
                    { "isShow", false },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", false },
                    { "status", "N" }
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
        [HttpPost("menu/read")]
        public ActionResult<Response> menuRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("mMenu");
                var filter = Builders<Register>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Eq("category", value.category); }
                var docs = col.Find(filter).SortBy(o => o.sequence).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.imageUrl, c.updateBy, c.updateDate, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.username, c.password, c.prefixName, c.firstName, c.lastName, c.phone, c.email, c.birthDay }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read ระดับการเข้าถึงเมนู
        [HttpPost("system/read")]
        public ActionResult<Response> systemRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("registerRole");

                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                filter &= Builders<Register>.Filter.Eq("username", value.username);

                var docs = col.Find(filter).SortByDescending(o => o.docDate).Project(c => new { c.category }).ToList();

                var category = new RegisterCategory
                {
                    createAction = false,
                    readAction = false,
                    updateAction = false,
                    deleteAction = false,
                    approveAction = false,

                    lv0 = "",
                    lv1 = "",
                    lv2 = "",
                    lv3 = "",
                    lv4 = "",

                    organizationPage = false,
                    userRolePage = false,
                    memberPage = false,
                    memberMobilePage = false,
                    personnelPage = false,
                    personnelStructurePage = false,

                    logoPage = false,
                    splashPage = false,
                    mainPopupPage = false,
                    bannerPage = false,
                    forceAdsPage = false,
                    rotationPage = false,
                    partnerPage = false,
                    alliancePage = false,
                    relateAgencyPage = false,
                    officeActivitiesPage = false,

                    newsPage = false,
                    importantPage = false,
                    eventPage = false,
                    contactPage = false,
                    knowledgePage = false,
                    knowledgeVetPage = false,
                    privilegePage = false,
                    poiPage = false,
                    pollPage = false,
                    suggestionPage = false,
                    notificationPage = false,
                    welfarePage = false,
                    trainingPage = false,
                    reporterPage = false,
                    warningPage = false,
                    fundPage = false,
                    cooperativeFormPage = false,
                    examinationPage = false,
                    imageEventPage = false,
                    eventAbroadPage = false,
                    vetEnewsPage = false,
                    newsCategoryPage = false,
                    importantCategoryPage = false,
                    eventCategoryPage = false,
                    contactCategoryPage = false,
                    knowledgeCategoryPage = false,
                    knowledgeVetCategoryPage = false,
                    privilegeCategoryPage = false,
                    poiCategoryPage = false,
                    pollCategoryPage = false,
                    suggestionCategoryPage = false,
                    notificationCategoryPage = false,
                    welfareCategoryPage = false,
                    trainingCategoryPage = false,
                    reporterCategoryPage = false,
                    warningCategoryPage = false,
                    fundCategoryPage = false,
                    cooperativeFormCategoryPage = false,
                    examinationCategoryPage = false,
                    imageEventCategoryPage = false,
                    eventAbroadCategoryPage = false,
                    policyApplicationPage = false,
                    policyMarketingPage = false,
                    memberMobilePolicyApplicationPage = false,
                    memberMobilePolicyMarketingPage = false,
                    vetEnewsCategoryPage = false,
                    websitevisitorPage = false,
                    cmsvisitorPage = false,
                    aboutCommentPage = false,
                    personnelStructureCategoryPage = false,
                    personnelStructureCategoryPage2 = false,
                    lawPage = false,
                    trainingInstitutePage = false,
                    lawCategoryPage = false,
                    trainingInstituteCategoryPage = false,
                    expertBranchPage = false,
                    expertBranchCategoryPage = false,
                    verifyApprovedUserPage = false,
                    verifyApprovedUserCategoryPage = false,
                    seminarPage = false,
                    seminarCategoryPage = false,

                    productPage = false,
                    productCategoryPage = false,
                    portfolioCategoryPage =false,

                    employeePage = false,
                    employeeCategoryPage = false,
                    certificatePage = false,
                    certificateCategoryPage = false,

                    workProcessPage = false,
                    portfolioPage = false,

                    //report
                    reportNumberMemberRegisterPage = false,
                    reportMemberRegisterPage = false,
                    reportNewsCategoryPage = false,
                    reportNewsPage = false,
                    reportKnowledgeCategoryPage = false,
                    reportKnowledgePage = false,
                    reportReporterPage = false,
                    reportContactPage = false,
                    reportEventCalendarPage = false,
                    reportPrivilegePage = false,
                    reportPoiPage = false,
                    reportPollPage = false,
                    reportReporterCreatePage = false,
                    reportWarningPage = false,
                    reportWelfarePage = false,

                    reportNewsKeysearchPage = false,
                    reportKnowledgeKeysearchPage = false,
                    reportEventCalendarKeysearchPage = false,
                    reportContactKeysearchPage = false,
                    reportPrivilegeKeysearchPage = false,
                    reportPoiKeysearchPage = false,
                    reportPollKeysearchPage = false,
                    reportReporterKeysearchPage = false,
                    reportWarningKeysearchPage = false,
                    reportWelfareKeysearchPage = false,

                    reportAboutUsPage = false,
                    reportForceAdsPage = false,
                    reportRotationPage = false,
                    //Master
                    swearWordsPage = false,
                    masterVeterinaryPage = false,
                    contentKeywordPage = false,
                    notificationResultPage = false,
                    notificationExamPage = false,
                    dashboardPage = false,
                };

                docs.ForEach(c =>
                {
                    var CategoryCol = new Database().MongoClient<RegisterCategory>( "registerCategory");

                    var CategoryFilter = Builders<RegisterCategory>.Filter.Eq(x => x.status, "A");
                    CategoryFilter &= Builders<RegisterCategory>.Filter.Eq("title", c.category);


                    var CategoryDoc = CategoryCol.Find(CategoryFilter).Project(c => new { c.createAction, c.readAction, c.updateAction, c.deleteAction, c.approveAction, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.organizationPage, c.userRolePage, c.memberPage,c.personnelPage,
                        c.aboutCommentPage,
                        c.personnelStructurePage, c.memberMobilePage, c.logoPage, c.splashPage, c.mainPopupPage, c.bannerPage,c.partnerPage, c.alliancePage, c.forceAdsPage,c.officeActivitiesPage, c.rotationPage, c.newsPage, c.importantPage, c.eventPage, c.contactPage, c.knowledgePage,c.knowledgeVetPage, c.privilegePage, c.poiPage, c.pollPage, c.examinationPage, c.suggestionPage, c.notificationPage, c.reporterPage, c.welfarePage, c.trainingPage, c.warningPage, c.newsCategoryPage, c.personnelStructureCategoryPage, c.personnelStructureCategoryPage2, c.importantCategoryPage, c.eventCategoryPage, c.contactCategoryPage, c.knowledgeCategoryPage,c.knowledgeVetCategoryPage, c.privilegeCategoryPage, c.poiCategoryPage, c.pollCategoryPage,c.examinationCategoryPage, c.suggestionCategoryPage, c.notificationCategoryPage, c.welfareCategoryPage, c.reporterCategoryPage, c.trainingCategoryPage, c.warningCategoryPage, c.policyMarketingPage ,c.policyApplicationPage, c.memberMobilePolicyApplicationPage, c.memberMobilePolicyMarketingPage, c.fundPage, c.fundCategoryPage, c.cooperativeFormPage, c.cooperativeFormCategoryPage,c.imageEventPage,c.imageEventCategoryPage,c.vetEnewsPage,c.vetEnewsCategoryPage,c.websitevisitorPage,c.cmsvisitorPage, c.lawPage, c.lawCategoryPage,c.trainingInstitutePage,c.trainingInstituteCategoryPage,
                        c.relateAgencyPage,
                        c.expertBranchPage,
                        c.expertBranchCategoryPage,
                        c.verifyApprovedUserPage,
                        c.verifyApprovedUserCategoryPage,
                        c.seminarPage,
                        c.seminarCategoryPage,
                        c.eventAbroadPage,
                        c.eventAbroadCategoryPage,

                        c.productPage,
                        c.productCategoryPage,

                        c.employeePage,
                        c.employeeCategoryPage,
                        c.certificatePage,
                        c.certificateCategoryPage,

                        c.workProcessPage,
                        c.portfolioPage,
                        c.portfolioCategoryPage,


                        //report
                        c.reportNumberMemberRegisterPage,
                        c.reportMemberRegisterPage,
                        c.reportNewsCategoryPage,
                        c.reportNewsPage,
                        c.reportKnowledgeCategoryPage,
                        c.reportKnowledgePage,
                        c.reportReporterPage,
                        c.reportContactPage,
                        c.reportEventCalendarPage,
                        c.reportPrivilegePage,
                        c.reportPoiPage,
                        c.reportPollPage,
                        c.reportReporterCreatePage,
                        c.reportWarningPage,
                        c.reportWelfarePage,

                        c.reportNewsKeysearchPage,
                        c.reportKnowledgeKeysearchPage,
                        c.reportEventCalendarKeysearchPage,
                        c.reportContactKeysearchPage,
                        c.reportPrivilegeKeysearchPage,
                        c.reportPoiKeysearchPage,
                        c.reportPollKeysearchPage,
                        c.reportReporterKeysearchPage,
                        c.reportWarningKeysearchPage,
                        c.reportWelfareKeysearchPage,

                        c.reportAboutUsPage,
                        c.reportForceAdsPage,
                        c.reportRotationPage,
                        //Master
                        c.swearWordsPage,
                        c.masterVeterinaryPage,
                        c.contentKeywordPage,
                        c.notificationResultPage,
                        c.notificationExamPage,
                        c.dashboardPage,
                    }).FirstOrDefault();

                    if (CategoryDoc != null)
                    {
                        if (CategoryDoc.createAction) { category.createAction = CategoryDoc.createAction; };
                        if (CategoryDoc.readAction) { category.readAction = CategoryDoc.readAction; };
                        if (CategoryDoc.updateAction) { category.updateAction = CategoryDoc.updateAction; };
                        if (CategoryDoc.deleteAction) { category.deleteAction = CategoryDoc.deleteAction; };
                        if (CategoryDoc.approveAction) { category.approveAction = CategoryDoc.approveAction; };

                        if (!string.IsNullOrEmpty(CategoryDoc.lv0))
                            if (string.IsNullOrEmpty(category.lv0))
                                category.lv0 = CategoryDoc.lv0;
                            else
                                category.lv0 = category.lv0 + "," + CategoryDoc.lv0;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv1))
                            if (string.IsNullOrEmpty(category.lv1))
                                category.lv1 = CategoryDoc.lv1;
                            else
                                category.lv1 = category.lv1 + "," + CategoryDoc.lv1;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv2))
                            if (string.IsNullOrEmpty(category.lv2))
                                category.lv2 = CategoryDoc.lv2;
                            else
                                category.lv2 = category.lv2 + "," + CategoryDoc.lv2;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv3))
                            if (string.IsNullOrEmpty(category.lv3))
                                category.lv3 = CategoryDoc.lv3;
                            else
                                category.lv3 = category.lv3 + "," + CategoryDoc.lv3;

                        if (!string.IsNullOrEmpty(CategoryDoc.lv4))
                            if (string.IsNullOrEmpty(category.lv4))
                                category.lv4 = CategoryDoc.lv4;
                            else
                                category.lv4 = category.lv4 + "," + CategoryDoc.lv4;


                        if (CategoryDoc.organizationPage) { category.organizationPage = CategoryDoc.organizationPage; };
                        if (CategoryDoc.userRolePage) { category.userRolePage = CategoryDoc.userRolePage; };
                        if (CategoryDoc.memberPage) { category.memberPage = CategoryDoc.memberPage; };
                        if (CategoryDoc.memberMobilePage) { category.memberMobilePage = CategoryDoc.memberMobilePage; };
                        if (CategoryDoc.personnelStructurePage) { category.personnelStructurePage = CategoryDoc.personnelStructurePage; };
                        if (CategoryDoc.personnelPage) { category.personnelPage = CategoryDoc.personnelPage; };

                        if (CategoryDoc.logoPage) { category.logoPage = CategoryDoc.logoPage; };
                        if (CategoryDoc.splashPage) { category.splashPage = CategoryDoc.splashPage; };
                        if (CategoryDoc.mainPopupPage) { category.mainPopupPage = CategoryDoc.mainPopupPage; };
                        if (CategoryDoc.bannerPage) { category.bannerPage = CategoryDoc.bannerPage; };
                        if (CategoryDoc.forceAdsPage) { category.forceAdsPage = CategoryDoc.forceAdsPage; };
                        if (CategoryDoc.rotationPage) { category.rotationPage = CategoryDoc.rotationPage; };
                        if (CategoryDoc.partnerPage) { category.partnerPage = CategoryDoc.partnerPage; };
                        if (CategoryDoc.alliancePage) { category.alliancePage = CategoryDoc.alliancePage; };
                        if (CategoryDoc.relateAgencyPage) { category.relateAgencyPage = CategoryDoc.relateAgencyPage; };
                        if (CategoryDoc.officeActivitiesPage) { category.officeActivitiesPage = CategoryDoc.officeActivitiesPage; };

                        if (CategoryDoc.newsPage) { category.newsPage = CategoryDoc.newsPage; };
                        if (CategoryDoc.importantPage) { category.importantPage = CategoryDoc.importantPage; };
                        if (CategoryDoc.eventPage) { category.eventPage = CategoryDoc.eventPage; };
                        if (CategoryDoc.contactPage) { category.contactPage = CategoryDoc.contactPage; };
                        if (CategoryDoc.knowledgePage) { category.knowledgePage = CategoryDoc.knowledgePage; };
                        if (CategoryDoc.knowledgeVetPage) { category.knowledgeVetPage = CategoryDoc.knowledgeVetPage; };
                        if (CategoryDoc.privilegePage) { category.privilegePage = CategoryDoc.privilegePage; };
                        if (CategoryDoc.poiPage) { category.poiPage = CategoryDoc.poiPage; };
                        if (CategoryDoc.pollPage) { category.pollPage = CategoryDoc.pollPage; };
                        if (CategoryDoc.suggestionPage) { category.suggestionPage = CategoryDoc.suggestionPage; };
                        if (CategoryDoc.notificationPage) { category.notificationPage = CategoryDoc.notificationPage; };
                        if (CategoryDoc.welfarePage) { category.welfarePage = CategoryDoc.welfarePage; };
                        if (CategoryDoc.trainingPage) { category.trainingPage = CategoryDoc.trainingPage; };
                        if (CategoryDoc.reporterPage) { category.reporterPage = CategoryDoc.reporterPage; };
                        if (CategoryDoc.warningPage) { category.warningPage = CategoryDoc.warningPage; };
                        if (CategoryDoc.fundPage) { category.fundPage = CategoryDoc.fundPage; };
                        if (CategoryDoc.cooperativeFormPage) { category.cooperativeFormPage = CategoryDoc.cooperativeFormPage; };
                        if (CategoryDoc.examinationPage) { category.examinationPage = CategoryDoc.examinationPage; };
                        if (CategoryDoc.imageEventPage) { category.imageEventPage = CategoryDoc.imageEventPage; };
                        if (CategoryDoc.eventAbroadPage) { category.eventAbroadPage = CategoryDoc.eventAbroadPage; };
                        if (CategoryDoc.vetEnewsPage) { category.vetEnewsPage = CategoryDoc.vetEnewsPage; };
                        if (CategoryDoc.lawPage) { category.lawPage = CategoryDoc.lawPage; };
                        if (CategoryDoc.trainingInstitutePage) { category.trainingInstitutePage = CategoryDoc.trainingInstitutePage; };
                        if (CategoryDoc.expertBranchPage) { category.expertBranchPage = CategoryDoc.expertBranchPage; };
                        if (CategoryDoc.verifyApprovedUserPage) { category.verifyApprovedUserPage = CategoryDoc.verifyApprovedUserPage; };
                        if (CategoryDoc.seminarPage) { category.seminarPage = CategoryDoc.seminarPage; };

                        if (CategoryDoc.productPage) { category.productPage = CategoryDoc.productPage; };
                        if (CategoryDoc.productCategoryPage) { category.productCategoryPage = CategoryDoc.productCategoryPage; };

                        if (CategoryDoc.employeePage) { category.employeePage = CategoryDoc.employeePage; };
                        if (CategoryDoc.employeeCategoryPage) { category.employeeCategoryPage = CategoryDoc.employeeCategoryPage; };

                        if (CategoryDoc.workProcessPage) { category.workProcessPage = CategoryDoc.workProcessPage; };
                        if (CategoryDoc.portfolioPage) { category.portfolioPage = CategoryDoc.portfolioPage; };
                        if (CategoryDoc.portfolioCategoryPage) { category.portfolioCategoryPage = CategoryDoc.portfolioCategoryPage; };

                        if (CategoryDoc.certificatePage) { category.certificatePage = CategoryDoc.certificatePage; };
                        if (CategoryDoc.certificateCategoryPage) { category.certificateCategoryPage = CategoryDoc.certificateCategoryPage; };

                        if (CategoryDoc.newsCategoryPage) { category.newsCategoryPage = CategoryDoc.newsCategoryPage; };
                        if (CategoryDoc.importantCategoryPage) { category.importantCategoryPage = CategoryDoc.importantCategoryPage; };
                        if (CategoryDoc.eventCategoryPage) { category.eventCategoryPage = CategoryDoc.eventCategoryPage; };
                        if (CategoryDoc.contactCategoryPage) { category.contactCategoryPage = CategoryDoc.contactCategoryPage; };
                        if (CategoryDoc.knowledgeCategoryPage) { category.knowledgeCategoryPage = CategoryDoc.knowledgeCategoryPage; };
                        if (CategoryDoc.knowledgeVetCategoryPage) { category.knowledgeVetCategoryPage = CategoryDoc.knowledgeVetCategoryPage; };
                        if (CategoryDoc.privilegeCategoryPage) { category.privilegeCategoryPage = CategoryDoc.privilegeCategoryPage; };
                        if (CategoryDoc.poiCategoryPage) { category.poiCategoryPage = CategoryDoc.poiCategoryPage; };
                        if (CategoryDoc.pollCategoryPage) { category.pollCategoryPage = CategoryDoc.pollCategoryPage; };
                        if (CategoryDoc.suggestionCategoryPage) { category.suggestionCategoryPage = CategoryDoc.suggestionCategoryPage; };
                        if (CategoryDoc.notificationCategoryPage) { category.notificationCategoryPage = CategoryDoc.notificationCategoryPage; };
                        if (CategoryDoc.welfareCategoryPage) { category.welfareCategoryPage = CategoryDoc.welfareCategoryPage; };
                        if (CategoryDoc.trainingCategoryPage) { category.trainingCategoryPage = CategoryDoc.trainingCategoryPage; };
                        if (CategoryDoc.reporterCategoryPage) { category.reporterCategoryPage = CategoryDoc.reporterCategoryPage; };
                        if (CategoryDoc.warningCategoryPage) { category.warningCategoryPage = CategoryDoc.warningCategoryPage; };
                        if (CategoryDoc.fundCategoryPage) { category.fundCategoryPage = CategoryDoc.fundCategoryPage; };
                        if (CategoryDoc.cooperativeFormCategoryPage) { category.cooperativeFormCategoryPage = CategoryDoc.cooperativeFormCategoryPage; };
                        if (CategoryDoc.examinationCategoryPage) { category.examinationCategoryPage = CategoryDoc.examinationCategoryPage; };
                        if (CategoryDoc.imageEventCategoryPage) { category.imageEventCategoryPage = CategoryDoc.imageEventCategoryPage; };
                        if (CategoryDoc.eventAbroadCategoryPage) { category.eventAbroadCategoryPage = CategoryDoc.eventAbroadCategoryPage; };
                        if (CategoryDoc.vetEnewsCategoryPage) { category.vetEnewsCategoryPage = CategoryDoc.vetEnewsCategoryPage; };
                        if (CategoryDoc.websitevisitorPage) { category.websitevisitorPage = CategoryDoc.websitevisitorPage; };
                        if (CategoryDoc.cmsvisitorPage) { category.cmsvisitorPage = CategoryDoc.cmsvisitorPage; };
                        if (CategoryDoc.aboutCommentPage) { category.aboutCommentPage = CategoryDoc.aboutCommentPage; };
                        if (CategoryDoc.personnelStructureCategoryPage) { category.personnelStructureCategoryPage = CategoryDoc.personnelStructureCategoryPage; };
                        if (CategoryDoc.personnelStructureCategoryPage2) { category.personnelStructureCategoryPage2 = CategoryDoc.personnelStructureCategoryPage2; };
                        if (CategoryDoc.lawCategoryPage) { category.lawCategoryPage = CategoryDoc.lawCategoryPage; };
                        if (CategoryDoc.expertBranchCategoryPage) { category.expertBranchCategoryPage = CategoryDoc.expertBranchCategoryPage; };
                        if (CategoryDoc.verifyApprovedUserCategoryPage) { category.verifyApprovedUserCategoryPage = CategoryDoc.verifyApprovedUserCategoryPage; };
                        if (CategoryDoc.trainingInstituteCategoryPage) { category.trainingInstituteCategoryPage = CategoryDoc.trainingInstituteCategoryPage; };
                        if (CategoryDoc.seminarCategoryPage) { category.seminarCategoryPage = CategoryDoc.seminarCategoryPage; };

                        if (CategoryDoc.policyApplicationPage) { category.policyApplicationPage = CategoryDoc.policyApplicationPage; };
                        if (CategoryDoc.policyMarketingPage) { category.policyMarketingPage = CategoryDoc.policyMarketingPage; };
                        if (CategoryDoc.memberMobilePolicyApplicationPage) { category.memberMobilePolicyApplicationPage = CategoryDoc.memberMobilePolicyApplicationPage; };
                        if (CategoryDoc.memberMobilePolicyMarketingPage) { category.memberMobilePolicyMarketingPage = CategoryDoc.memberMobilePolicyMarketingPage; };
                        //report
                        if (CategoryDoc.reportNumberMemberRegisterPage) { category.reportNumberMemberRegisterPage = CategoryDoc.reportNumberMemberRegisterPage; };
                        if (CategoryDoc.reportMemberRegisterPage) { category.reportMemberRegisterPage = CategoryDoc.reportMemberRegisterPage; };
                        if (CategoryDoc.reportNewsCategoryPage) { category.reportNewsCategoryPage = CategoryDoc.reportNewsCategoryPage; };
                        if (CategoryDoc.reportNewsPage) { category.reportNewsPage = CategoryDoc.reportNewsPage; };
                        if (CategoryDoc.reportKnowledgeCategoryPage) { category.reportKnowledgeCategoryPage = CategoryDoc.reportKnowledgeCategoryPage; };
                        if (CategoryDoc.reportKnowledgePage) { category.reportKnowledgePage = CategoryDoc.reportKnowledgePage; };
                        if (CategoryDoc.reportReporterPage) { category.reportReporterPage = CategoryDoc.reportReporterPage; };
                        if (CategoryDoc.reportContactPage) { category.reportContactPage = CategoryDoc.reportContactPage; };
                        if (CategoryDoc.reportEventCalendarPage) { category.reportEventCalendarPage = CategoryDoc.reportEventCalendarPage; };
                        if (CategoryDoc.reportPrivilegePage) { category.reportPrivilegePage = CategoryDoc.reportPrivilegePage; };
                        if (CategoryDoc.reportPoiPage) { category.reportPoiPage = CategoryDoc.reportPoiPage; };
                        if (CategoryDoc.reportPollPage) { category.reportPollPage = CategoryDoc.reportPollPage; };
                        if (CategoryDoc.reportReporterCreatePage) { category.reportReporterCreatePage = CategoryDoc.reportReporterCreatePage; };
                        if (CategoryDoc.reportWarningPage) { category.reportWarningPage = CategoryDoc.reportWarningPage; };
                        if (CategoryDoc.reportWelfarePage) { category.reportWelfarePage = CategoryDoc.reportWelfarePage; };

                        if (CategoryDoc.reportNewsKeysearchPage) { category.reportNewsKeysearchPage = CategoryDoc.reportNewsKeysearchPage; };
                        if (CategoryDoc.reportKnowledgeKeysearchPage) { category.reportKnowledgeKeysearchPage = CategoryDoc.reportKnowledgeKeysearchPage; };
                        if (CategoryDoc.reportEventCalendarKeysearchPage) { category.reportEventCalendarKeysearchPage = CategoryDoc.reportEventCalendarKeysearchPage; };
                        if (CategoryDoc.reportContactKeysearchPage) { category.reportContactKeysearchPage = CategoryDoc.reportContactKeysearchPage; };
                        if (CategoryDoc.reportPrivilegeKeysearchPage) { category.reportPrivilegeKeysearchPage = CategoryDoc.reportPrivilegeKeysearchPage; };
                        if (CategoryDoc.reportPoiKeysearchPage) { category.reportPoiKeysearchPage = CategoryDoc.reportPoiKeysearchPage; };
                        if (CategoryDoc.reportPollKeysearchPage) { category.reportPollKeysearchPage = CategoryDoc.reportPollKeysearchPage; };
                        if (CategoryDoc.reportReporterKeysearchPage) { category.reportReporterKeysearchPage = CategoryDoc.reportReporterKeysearchPage; };
                        if (CategoryDoc.reportWarningKeysearchPage) { category.reportWarningKeysearchPage = CategoryDoc.reportWarningKeysearchPage; };
                        if (CategoryDoc.reportWelfareKeysearchPage) { category.reportWelfareKeysearchPage = CategoryDoc.reportWelfareKeysearchPage; };

                        if (CategoryDoc.reportAboutUsPage) { category.reportAboutUsPage = CategoryDoc.reportAboutUsPage; };
                        if (CategoryDoc.reportForceAdsPage) { category.reportForceAdsPage = CategoryDoc.reportForceAdsPage; };
                        if (CategoryDoc.reportRotationPage) { category.reportRotationPage = CategoryDoc.reportRotationPage; };
                        //Master
                        if (CategoryDoc.swearWordsPage) { category.swearWordsPage = CategoryDoc.swearWordsPage; };
                        if (CategoryDoc.masterVeterinaryPage) { category.masterVeterinaryPage = CategoryDoc.masterVeterinaryPage; };
                        if (CategoryDoc.contentKeywordPage) { category.contentKeywordPage = CategoryDoc.contentKeywordPage; };
                        if (CategoryDoc.notificationResultPage) { category.notificationResultPage = CategoryDoc.notificationResultPage; };
                        if (CategoryDoc.notificationExamPage) { category.notificationExamPage = CategoryDoc.notificationExamPage; };
                        if (CategoryDoc.dashboardPage) { category.dashboardPage = CategoryDoc.dashboardPage; };

                    }
                });

                //category.lv0 = category.lv0.Substring(1);
                //category.lv1 = category.lv1.Substring(1);
                //category.lv2 = category.lv2.Substring(1);
                //category.lv3 = category.lv3.Substring(1);
                //category.lv4 = category.lv4.Substring(1);

                return new Response { status = "S", message = "success", jsonData = category.ToJson(), objectData = category };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read ระดับการเปิดปิดปุ่ม การเห็นข้อมูล
        [HttpPost("page/read")]
        public ActionResult<Response> pageRead([FromBody] Criteria value)
        {
            try
            {
                var dataList = new List<RegisterCategory>();

                //page นี้ อยู่ใน role ไหนบ้าง จากนั้นก็ดูว่า แต่ละ category ทำอะไรได้บ้าง

                // title = หน้าจอ
                // createBy = username

                var col = new Database().MongoClient<Register>( "registerRole");
                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                filter &= Builders<Register>.Filter.Eq("username", value.updateBy);
                var docs = col.Find(filter).SortByDescending(o => o.docDate).Project(c => new { c.category }).ToList();

                docs.ForEach(c =>
                {
                    var CategoryCol = new Database().MongoClient<RegisterCategory>( "registerCategory");

                    var CategoryFilter = Builders<RegisterCategory>.Filter.Eq(x => x.isActive, true);
                    CategoryFilter &= Builders<RegisterCategory>.Filter.Eq("title", c.category);

                    switch (value.title)
                    {
                        case "newsPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.newsPage, true);
                            break;
                        case "importantPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.importantPage, true);
                            break;
                        case "eventPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.eventPage, true);
                            break;
                        case "contactPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.contactPage, true);
                            break;
                        case "knowledgePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.knowledgePage, true);
                            break;
                        case "knowledgeVetPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.knowledgeVetPage, true);
                            break;
                        case "privilegePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.privilegePage, true);
                            break;
                        case "poiPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.poiPage, true);
                            break;
                        case "pollPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.pollPage, true);
                            break;
                        case "suggestionPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.suggestionPage, true);
                            break;
                        case "notificationPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.notificationPage, true);
                            break;
                        case "welfarePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.welfarePage, true);
                            break;
                        case "trainingPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.trainingPage, true);
                            break;
                        case "reporter":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.reporterPage, true);
                            break;
                        case "warning":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.warningPage, true);
                            break;
                        case "fund":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.fundPage, true);
                            break;
                        case "cooperativeForm":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.cooperativeFormPage, true);
                            break;
                        case "examinationPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.examinationPage, true);
                            break;
                        case "imageEventPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.imageEventPage, true);
                            break;
                        case "eventAbroadPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.eventAbroadPage, true);
                            break;
                        case "vetEnewsPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.vetEnewsPage, true);
                            break;
                        case "lawPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.lawPage, true);
                            break;
                        case "expertBranchPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.expertBranchPage, true);
                            break;
                        case "verifyApprovedUserPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.verifyApprovedUserPage, true);
                            break;
                        case "trainingInstitutePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.trainingInstitutePage, true);
                            break;
                        case "seminarPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.seminarPage, true);
                            break;
                        case "productPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.productPage, true);
                            break;
                        case "employeePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.employeePage, true);
                            break;
                        case "workProcessPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.workProcessPage, true);
                            break;
                        case "portfolioPage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.portfolioPage, true);
                            break;
                        case "certificatePage":
                            CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.certificatePage, true);
                            break;
                        default:
                            break;
                    }

                   

                    var categoryDocs = CategoryCol.Find(CategoryFilter).Project(c => new { c.code, c.createAction, c.readAction, c.updateAction, c.deleteAction, c.approveAction }).ToList();

                    //ไปหาใน RegisterPermission ต่อว่ามี category อะไรบ้าง
                    categoryDocs.ForEach(c => {
                        var permissionCol = new Database().MongoClient<Permission>( "registerPermission");
                        var permissionFilter = Builders<Permission>.Filter.Eq(x => x.isActive, true);
                        permissionFilter &= Builders<Permission>.Filter.Eq("reference", c.code);

                        switch (value.title)
                        {
                            case "newsPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.newsPage, true);
                                break;
                            case "importantPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.importantPage, true);
                                break;
                            case "eventPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.eventPage, true);
                                break;
                            case "contactPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.contactPage, true);
                                break;
                            case "knowledgePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.knowledgePage, true);
                                break;
                            case "knowledgeVetPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.knowledgeVetPage, true);
                                break;
                            case "privilegePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.privilegePage, true);
                                break;
                            case "poiPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.poiPage, true);
                                break;
                            case "pollPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.pollPage, true);
                                break;
                            case "suggestionPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.suggestionPage, true);
                                break;
                            case "notificationPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.notificationPage, true);
                                break;
                            case "welfarePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.welfarePage, true);
                                break;
                            case "trainingPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.trainingPage, true);
                                break;
                            case "reporterPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.reporterPage, true);
                                break;
                            case "warningPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.warningPage, true);
                                break;
                            case "fundPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.fundPage, true);
                                break;
                            case "cooperativeFormPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.cooperativeFormPage, true);
                                break;
                            case "examinationPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.examinationPage, true);
                                break;
                            case "imageEventPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.imageEventPage, true);
                                break;
                            case "eventAbroadPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.eventAbroadPage, true);
                                break;
                            case "vetEnewsPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.vetEnewsPage, true);
                                break;
                            case "lawPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.lawPage, true);
                                break;
                            case "expertBranchPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.expertBranchPage, true);
                                break;
                            case "verifyApprovedUserPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.verifyApprovedUserPage, true);
                                break;
                            case "trainingInstitutePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.trainingInstitutePage, true);
                                break;
                            case "seminarPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.seminarPage, true);
                                break;
                            case "productPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.productPage, true);
                                break;
                            case "employeePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.employeePage, true);
                                break;
                            case "workProcessPage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.workProcessPage, true);
                                break;
                            case "portfolioPage":
                                CategoryFilter &= Builders<RegisterCategory>.Filter.Eq(x => x.portfolioPage, true);
                                break;
                            case "certificatePage":
                                permissionFilter &= Builders<Permission>.Filter.Eq(x => x.certificatePage, true);
                                break;
                            default:
                                break;
                        }

                        //เซต action  
                        var permissionDocs = permissionCol.Find(permissionFilter).SortByDescending(o => o.docDate).Project(c => new { c.page, c.category }).ToList();
                        permissionDocs.ForEach(cc => {

                            var isExist = dataList.FirstOrDefault(c => c.title == cc.category);

                            if (isExist != null)
                            {
                                if (c.createAction) { isExist.createAction = c.createAction; }
                                if (c.readAction) { isExist.readAction = c.readAction; }
                                if (c.updateAction) { isExist.updateAction = c.updateAction; }
                                if (c.deleteAction) { isExist.deleteAction = c.deleteAction; }
                                if (c.approveAction) { isExist.approveAction = c.approveAction; }
                            }
                            else
                            {
                                dataList.Add(new RegisterCategory { title = cc.category, createAction = c.createAction, readAction = c.readAction, updateAction = c.updateAction, deleteAction = c.deleteAction, approveAction = c.approveAction });
                            }
                        });
                    });
                });

                return new Response { status = "S", message = "success", jsonData = dataList.ToJson(), objectData = dataList };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("organization/read")]
        public ActionResult<Response> OrganizationRead([FromBody] Criteria value)
        {
            try
            {
                var model = new List<Object>();

                var col = new Database().MongoClient<Register>("registerRole");
                var filter = Builders<Register>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", value.username); }
                var docs = col.Find(filter).Project(c => new { c.category }).ToList();
                docs.ForEach(c =>
                {
                    var col2 = new Database().MongoClient<RegisterCategory>("registerCategory");
                    var filter2 = Builders<RegisterCategory>.Filter.Ne(x => x.status, "D") & Builders<RegisterCategory>.Filter.Eq(x => x.title, c.category);
                    var docs2 = col2.Find(filter2).Project(c => new { c.title, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.status }).ToList();
                    docs2.ForEach(cc =>
                    {
                        model.Add(new
                        {
                            title = cc.title,
                            lv0 = cc.lv0,
                            lv1 = cc.lv1,
                            lv2 = cc.lv2,
                            lv3 = cc.lv3,
                            lv4 = cc.lv4,
                            status = "A"
                        });
                    });
                });

                return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /read
        [HttpPost("organization/check")]
        public ActionResult<Response> OrganizationCheck([FromBody] Criteria value)
        {
            try
            {
                var isExist = true;

                var model = new List<Object>();

                value.organization.ForEach(c =>
                {

                    if (!string.IsNullOrEmpty(c.lv0))
                    {
                        var split = c.lv0.Split(",");

                        foreach (var item in split)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                var col = new Database().MongoClient<News>("organization");
                                var filter = Builders<News>.Filter.Eq(x => x.code, item);

                                if (isExist)
                                    isExist = col.Find(filter).Any();
                            }
                        }
                    }
                });

                if (!isExist)
                    return new Response { status = "E" };
                else
                    return new Response { status = "S", message = "success", jsonData = model.ToJson(), objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        #endregion

        #region member

        // POST /read
        [HttpPost("member/read")]
        public ActionResult<Response> MemberRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Register>("register");
                var filter = (Builders<Register>.Filter.Ne("status", "D")) & Builders<Register>.Filter.Ne(x => x.category, "");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.status))
                    {
                        if (value.status == "VR")
                        {
                            filter = (Builders<Register>.Filter.Eq("status", "V") | Builders<Register>.Filter.Eq("status", "R"));
                        }
                        else
                        {
                            filter = filter & Builders<Register>.Filter.Eq("status", value.status);
                        }
                    }

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }

                    var sv = new AES();
                    var encryptPassword = "";
                    if (!string.IsNullOrEmpty(value.password))
                        encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Eq("password", encryptPassword); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Register>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.sex)) { filter = filter & Builders<Register>.Filter.Regex("sex", value.sex); }
                    if (!string.IsNullOrEmpty(value.lineID)) { filter = filter & Builders<Register>.Filter.Regex("lineID", value.lineID); }
                    if (!string.IsNullOrEmpty(value.line)) { filter = filter & Builders<Register>.Filter.Regex("line", value.line); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter)
                    .SortByDescending(o => o.docDate)
                    .ThenByDescending(o => o.updateTime)
                    .Skip(value.skip)
                    .Limit(value.limit)
                    .Project(c => new
                    {
                        c.code,
                        c.username,
                        c.password,
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
                        c.line,
                        c.sex,
                        c.soi,
                        c.address,
                        c.moo,
                        c.road,
                        c.tambonCode,
                        c.tambon,
                        c.amphoeCode,
                        c.amphoe,
                        c.provinceCode,
                        c.province,
                        c.postnoCode,
                        c.postno,
                        c.job,
                        c.idcard,
                        c.officerCode,
                        c.licenseNumber,
                        c.countUnit,
                        //countUnit = string.IsNullOrEmpty(c.countUnit) || c.countUnit != "{}" || c.countUnit != "[]" ? JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<CountUnit>>(c.countUnit).Where(c => c.status != "D").ToList()) : "",
                        c.status,
                        c.lv0,
                        c.lv1,
                        c.lv2,
                        c.lv3,
                        c.lv4,
                        c.linkAccount

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
        [HttpPost("member/update")]
        public ActionResult<Response> MemberUpdate([FromBody] Register value)
        {
            var doc = new BsonDocument();
            try
            {
                value.logCreate("register/update", value.code);

                if (!string.IsNullOrEmpty(value.category) && !string.IsNullOrEmpty(value.code))
                {
                    var col2 = new Database().MongoClient<Register>("register");
                    var filter2 = Builders<Register>.Filter.Ne("status", "D") & Builders<Register>.Filter.Eq("code", value.code);
                    var doc2 = col2.Find(filter2).Project(c => new { c.countUnit }).FirstOrDefault();

                    var dataCountUnit = "";
                    if (string.IsNullOrEmpty(doc2.countUnit))
                    {
                        dataCountUnit = "[]";
                    }
                    else
                    {
                        dataCountUnit = doc2.countUnit;
                    }

                    List<CountUnit> countUnitDB = JsonConvert.DeserializeObject<List<CountUnit>>(dataCountUnit); // count unit from database
                    List<CountUnit> countUnitValue = JsonConvert.DeserializeObject<List<CountUnit>>(value.countUnit); // count unit from value

                    //var countUnitString = "[]";
                    if (countUnitDB.Count != countUnitValue.Count)
                    {
                        if (countUnitValue.Count == 0)
                        {
                            value.isActive = false;
                            value.status = "N";
                        }
                        else if (countUnitValue.Count > 0)
                        {
                            for (int i = 0; i < countUnitValue.Count; i++)
                            {
                                if (countUnitValue[i].status != "A")
                                {
                                    value.isActive = false;
                                    value.status = "V";
                                    break;
                                }
                                else
                                {
                                    value.isActive = true;
                                    value.status = "A";
                                }
                            }

                            //countUnitString = JsonConvert.DeserializeObject<List<CountUnit>>(value.countUnit);
                        }
                    }
                }

                //if (col2.Find(filter2).Any())
                //{
                //    return new Response { status = "E", message = $"email: {value.email} is exist", jsonData = value.email, objectData = value.email };
                //}

                // set status via countUnit. jiravong

                //var filter3 = Builders<BsonDocument>.Filter.Eq("idcard", value.idcard);
                //if (col2.Find(filter3).Any())
                //{
                //    //return new Response { status = "E", message = $"idcard: {value.idcard} is exist", jsonData = value.ToJson(), objectData = value };
                //    return new Response { status = "E", message = $"รหัสบัตรประชาชน {value.idcard} ได้ทำการยืนยันตัวตนเรียบร้อยแล้ว กรุณาตรวจสอบใหม่อีกครั้ง", jsonData = value.ToJson(), objectData = value };

                //}

                //if (!string.IsNullOrEmpty(value.countUnit))
                //{
                //    try
                //    {
                //        List<CountUnit> countUnit = JsonConvert.DeserializeObject<List<CountUnit>>(value.countUnit);

                //        if (countUnit.Count > 0)
                //        {
                //            for (int i = 0; i < countUnit.Count; i++)
                //            {
                //                if (countUnit[i].status != "A")
                //                {
                //                    value.isActive = false;
                //                    value.status = "N";
                //                    break;
                //                }
                //                else
                //                {
                //                    value.isActive = true;
                //                    value.status = "A";
                //                }
                //            }
                //        }
                //        else
                //        {
                //            value.isActive = false;
                //            value.status = "N";
                //        }
                //    }
                //    catch { value.countUnit = "[]"; };

                //}

                // set status via countUnit

                // start update linkAccount
                var col = new Database().MongoClient("register");

                if (string.IsNullOrEmpty(value.linkAccount) && !string.IsNullOrEmpty(value.code))
                {
                    var filter = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = col.Find(filter).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);

                    var json = JsonConvert.SerializeObject(model);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logRegister");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "updateLinkAccount" },
                        { "raw", json },
                        { "createBy", value.updateBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "status", "A" }
                    };
                    col1.InsertOne(doc1);

                    if (value.category != "guest")
                    {
                        doc["username"] = value.username;
                    }
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
                    doc["line"] = value.line ?? "";
                    doc["password"] = value.password ?? "";
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
                    doc["job"] = value.job ?? "";
                    doc["idcard"] = value.idcard ?? "";
                    doc["officerCode"] = value.officerCode ?? "";
                    doc["licenseNumber"] = value.licenseNumber ?? "";
                    doc["countUnit"] = value.countUnit ?? "";
                    doc["lv0"] = value.lv0 ?? "";
                    doc["lv1"] = value.lv1 ?? "";
                    doc["lv2"] = value.lv2 ?? "";
                    doc["lv3"] = value.lv3 ?? "";
                    doc["lv4"] = value.lv4 ?? "";
                    doc["linkAccount"] = value.linkAccount ?? "";
                    doc["description"] = value.description ?? "";
                    doc["isActive"] = value.isActive;
                    doc["status"] = value.status;
                    doc["updateBy"] = value.updateBy;
                    doc["updateDate"] = DateTime.Now.toStringFromDate();
                    col.ReplaceOne(filter, doc);
                }
                else if (!string.IsNullOrEmpty(value.linkAccount))
                {
                    var collectionPermission = new Database().MongoClient("register");
                    var filterPermission = Builders<BsonDocument>.Filter.Ne("status", "D") & Builders<BsonDocument>.Filter.Eq("linkAccount", value.linkAccount);

                    var json = JsonConvert.SerializeObject(filterPermission);
                    var doc1 = new BsonDocument();
                    var col1 = new Database().MongoClient("_logRegister");
                    doc1 = new BsonDocument
                    {
                        { "code", "".toCode() },
                        { "step", "updateLinkAccount" },
                        { "raw", json },
                        { "createBy", value.updateBy },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", value.updateBy },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true },
                        { "status", "A" }
                    };
                    col1.InsertOne(doc1);


                    var updatePermission = Builders<BsonDocument>.Update
                    .Set("imageUrl", value.imageUrl)
                    .Set("prefixName", value.prefixName)
                    .Set("firstName", value.firstName)
                    .Set("lastName", value.lastName)
                    .Set("birthDay", value.birthDay)
                    .Set("phone", value.phone)
                    .Set("facebookID", value.facebookID)
                    .Set("googleID", value.googleID)
                    .Set("lineID", value.lineID)
                    .Set("line", value.line)
                    .Set("password", value.password)
                    .Set("email", value.email)
                    .Set("sex", value.sex)
                    .Set("soi", value.soi)
                    .Set("address", value.address)
                    .Set("moo", value.moo)
                    .Set("road", value.road)
                    .Set("tambonCode", value.tambonCode)
                    .Set("tambon", value.tambon)
                    .Set("amphoeCode", value.amphoeCode)
                    .Set("amphoe", value.amphoe)
                    .Set("province", value.province)
                    .Set("provinceCode", value.provinceCode)
                    .Set("postnoCode", value.postnoCode)
                    .Set("postno", value.postno)
                    .Set("job", value.job)
                    .Set("idcard", value.idcard)
                    .Set("officerCode", value.officerCode)
                    .Set("licenseNumber", value.licenseNumber)
                    .Set("countUnit", value.countUnit)
                    .Set("lv0", value.lv0)
                    .Set("lv1", value.lv1)
                    .Set("lv2", value.lv2)
                    .Set("lv3", value.lv3)
                    .Set("lv4", value.lv4)
                    .Set("isActive", value.isActive)
                    .Set("description", value.description)
                    .Set("status", value.status)
                    .Set("updateBy", value.updateBy)
                    .Set("updateDate", DateTime.Now.toStringFromDate());
                    collectionPermission.UpdateMany(filterPermission, updatePermission);

                }
                // end update linkAccount

                //BEGIN :read >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                var registerCol = new Database().MongoClient<Register>("register");
                var registerFilter = Builders<Register>.Filter.Eq("code", value.code);
                var registerDoc = registerCol.Find(registerFilter).Project(c => new
                {
                    c.code,
                    c.username,
                    c.password,
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
                    c.line,
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
                    c.job,
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

                //END :read <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


                return new Response { status = "S", message = "success", jsonData = registerDoc.ToJson(), objectData = registerDoc };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("member/delete")]
        public ActionResult<Response> MemberDelete([FromBody] Register value)
        {
            try
            {
                var col = new Database().MongoClient("register");

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

        #region report

        // POST /login
        [HttpPost("report/read")]
        public ActionResult<Response> ReportRead([FromBody] Criteria value)
        {
            try
            {
                List<object> result = new List<object>(); // model report

                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Ne("status", "D") & Builders<Register>.Filter.Eq("category", "");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }
                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Regex("password", value.password); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                List<Register> docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Project(c => new Register
                {
                    code = c.code,
                    username = c.username,
                    firstName = c.firstName,
                    lastName = c.lastName,
                    password = c.password,
                    category = c.category,
                    imageUrl = c.imageUrl,
                    updateDate = c.updateDate,
                    updateTime = c.updateTime,
                    createDate = c.createDate,
                    createTime = c.createTime,
                }).ToList();

                // get name user role.
                var colRole = new Database().MongoClient<registerRole>("registerRole");
                var filterRole = Builders<registerRole>.Filter.Eq(x => x.isActive, true);
                //if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", c.username); }
                var registerRole = colRole.Find(filterRole).Project(c => new { c.username, c.category }).ToList();

                // get register category receive lv organize.
                var colRegisterCat = new Database().MongoClient<RegisterCategory>("registerCategory");
                var filterRegisterCat = Builders<RegisterCategory>.Filter.Eq(x => x.isActive, true);
                //if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", c.username); }
                var registerCategory = colRegisterCat.Find(filterRegisterCat).Project(c => new { c.title, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5 }).ToList();

                // get organization
                var colOg = new Database().MongoClient<Register>("organization");
                var filterOg = Builders<Register>.Filter.Eq(x => x.isActive, true);
                var docOg = colOg.Find(filterOg).Project(c => new { c.code, c.title, c.cpTitle, c.cpCode, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5 }).ToList();

                result = new List<object>();
                var order = 1;
                docs.ForEach(c =>
                {
                    var currentUser = "";
                    // get name user role.
                    var docsRole = registerRole.Where(w => w.username == c.username).ToList();

                    //c.lv0,c.lv1,c.lv2,c.lv3,c.lv4,c.lv5

                    if (docsRole.Count > 0)
                        docsRole.ForEach(cc =>
                        {
                            // get register category receive lv organize.
                            var docsRegisterCat = registerCategory.FirstOrDefault(f => f.title == cc.category);

                            try
                            {

                                var colOg = new Database().MongoClient<Register>("organization");

                                var listLv0 = !string.IsNullOrEmpty(docsRegisterCat.lv0) ? docsRegisterCat.lv0.Split(",") : new string[0];
                                var listLv1 = !string.IsNullOrEmpty(docsRegisterCat.lv1) ? docsRegisterCat.lv1.Split(",") : new string[0];
                                var listLv2 = !string.IsNullOrEmpty(docsRegisterCat.lv2) ? docsRegisterCat.lv2.Split(",") : new string[0];
                                var listLv3 = !string.IsNullOrEmpty(docsRegisterCat.lv3) ? docsRegisterCat.lv3.Split(",") : new string[0];
                                var listLv4 = !string.IsNullOrEmpty(docsRegisterCat.lv4) ? docsRegisterCat.lv4.Split(",") : new string[0];
                                var listLv5 = !string.IsNullOrEmpty(docsRegisterCat.lv5) ? docsRegisterCat.lv5.Split(",") : new string[0];

                                foreach (var lv0 in listLv0)
                                {
                                    var docLv0 = docOg.FirstOrDefault(f => f.code == lv0);
                                    // lv1
                                    if (listLv1.Length > 0)
                                    {
                                        foreach (var lv1 in listLv1)
                                        {
                                            var docLv1 = docOg.FirstOrDefault(f => f.code == lv1 && c.lv0 == lv0);
                                            if (docLv1 != null && listLv2.Length > 0)
                                            {
                                                foreach (var lv2 in listLv2)
                                                {
                                                    var docLv2 = docOg.FirstOrDefault(f => f.code == lv2 && f.lv1 == lv1);
                                                    if (docLv2 != null && listLv3.Length > 0)
                                                    {
                                                        foreach (var lv3 in listLv3)
                                                        {
                                                            var docLv3 = docOg.FirstOrDefault(f => f.code == lv3 && f.lv2 == lv2);
                                                            if (docLv3 != null && listLv4.Length > 0)
                                                            {
                                                                foreach (var lv4 in listLv4)
                                                                {
                                                                    var docLv4 = docOg.FirstOrDefault(f => f.code == lv4 && f.lv3 == lv3);
                                                                    if (docLv4 != null && listLv5.Length > 0)
                                                                    {
                                                                        foreach (var lv5 in listLv5)
                                                                        {
                                                                            var sameOrder = order;
                                                                            var filterLv5 = Builders<Register>.Filter.Eq(x => x.isActive, true) & Builders<Register>.Filter.Eq("code", lv5) & Builders<Register>.Filter.Eq("lv4", lv4);
                                                                            var docLv5 = colOg.Find(filterLv5).Project(c => new { c.title }).FirstOrDefault();
                                                                            //if (c.username != currentUser) sameOrder = "";
                                                                            result.Add(new
                                                                            {
                                                                                order = order,
                                                                                code = c.code,
                                                                                username = c.username,
                                                                                firstName = c.firstName,
                                                                                lastName = c.lastName,
                                                                                category = cc.category,
                                                                                lv0 = lv0,
                                                                                titleLv0 = docLv0.title,
                                                                                lv1 = lv1,
                                                                                titleLv1 = docLv1.title,
                                                                                lv2 = lv2,
                                                                                titleLv2 = docLv2.title,
                                                                                lv3 = lv3,
                                                                                titleLv3 = docLv3.title,
                                                                                lv4 = lv4,
                                                                                titleLv4 = docLv4.title,
                                                                                lv5 = (!string.IsNullOrEmpty(lv5)) ? lv5 : "",
                                                                                titleLv5 = (!string.IsNullOrEmpty(docLv5.title)) ? docLv5.title : "",
                                                                                cpTitle = docLv2 != null ? docLv2?.cpTitle : "",
                                                                            });
                                                                            if (c.username != currentUser) order++;

                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        result.Add(new
                                                                        {
                                                                            order = order,
                                                                            code = c.code,
                                                                            username = c.username,
                                                                            firstName = c.firstName,
                                                                            lastName = c.lastName,
                                                                            category = cc.category,
                                                                            lv0 = lv0,
                                                                            titleLv0 = docLv0.title,
                                                                            lv1 = lv1,
                                                                            titleLv1 = docLv1.title,
                                                                            lv2 = lv2,
                                                                            titleLv2 = docLv2.title,
                                                                            lv3 = lv3,
                                                                            titleLv3 = docLv3.title,
                                                                            lv4 = lv4,
                                                                            titleLv4 = docLv4.title != null ? docLv4.title : "",
                                                                            lv5 = "",
                                                                            titleLv5 = "",
                                                                            cpTitle = docLv2 != null ? docLv2?.cpTitle : "",
                                                                        });
                                                                        if (c.username != currentUser) order++;
                                                                    }

                                                                }
                                                            }
                                                            else
                                                            {
                                                                result.Add(new
                                                                {
                                                                    order = order,
                                                                    code = c.code,
                                                                    username = c.username,
                                                                    firstName = c.firstName,
                                                                    lastName = c.lastName,
                                                                    category = cc.category,
                                                                    lv0 = lv0,
                                                                    titleLv0 = docLv0.title,
                                                                    lv1 = lv1,
                                                                    titleLv1 = docLv1.title,
                                                                    lv2 = lv2,
                                                                    titleLv2 = docLv2.title,
                                                                    lv3 = lv3,
                                                                    titleLv3 = docLv3.title != null ? docLv3.title : "",
                                                                    lv4 = "",
                                                                    titleLv4 = "",
                                                                    lv5 = "",
                                                                    titleLv5 = "",
                                                                    cpTitle = docLv2 != null ? docLv2?.cpTitle : "",
                                                                }); ;
                                                                if (c.username != currentUser) order++;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        result.Add(new
                                                        {
                                                            order = order,
                                                            code = c.code,
                                                            username = c.username,
                                                            firstName = c.firstName,
                                                            lastName = c.lastName,
                                                            category = cc.category,
                                                            lv0 = lv0,
                                                            titleLv0 = docLv0.title,
                                                            lv1 = lv1,
                                                            titleLv1 = docLv1.title,
                                                            lv2 = lv2,
                                                            titleLv2 = docLv2.title != null ? docLv2.title : "",
                                                            lv3 = "",
                                                            titleLv3 = "",
                                                            lv4 = "",
                                                            titleLv4 = "",
                                                            lv5 = "",
                                                            titleLv5 = "",
                                                            cpTitle = docLv2 != null ? docLv2?.cpTitle : "",
                                                        });
                                                        if (c.username != currentUser) order++;
                                                    }


                                                }
                                            }
                                            else
                                            {
                                                result.Add(new
                                                {
                                                    order = order,
                                                    code = c.code,
                                                    username = c.username,
                                                    firstName = c.firstName,
                                                    lastName = c.lastName,
                                                    category = cc.category,
                                                    lv0 = lv0,
                                                    titleLv0 = docLv0.title,
                                                    lv1 = lv1,
                                                    titleLv1 = docLv1.title != null ? docLv1.title : "",
                                                    lv2 = "",
                                                    titleLv2 = "",
                                                    lv3 = "",
                                                    titleLv3 = "",
                                                    lv4 = "",
                                                    titleLv4 = "",
                                                    lv5 = "",
                                                    titleLv5 = "",
                                                    cpTitle = "",
                                                });
                                                if (c.username != currentUser) order++;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        result.Add(new
                                        {
                                            order = order,
                                            code = c.code,
                                            username = c.username,
                                            firstName = c.firstName,
                                            lastName = c.lastName,
                                            category = cc.category,
                                            lv0 = lv0,
                                            titleLv0 = docLv0.title,
                                            lv1 = "",
                                            titleLv1 = "",
                                            lv2 = "",
                                            titleLv2 = "",
                                            lv3 = "",
                                            titleLv3 = "",
                                            lv4 = "",
                                            titleLv4 = "",
                                            lv5 = "",
                                            titleLv5 = "",
                                            cpTitle = "",
                                        });
                                        if (c.username != currentUser) order++;
                                    }

                                    currentUser = c.username;
                                }
                            }
                            catch { }

                        });

                });

                return new Response { status = "S", message = "success", jsonData = result.ToJson(), objectData = result, totalData = result.Count() };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /login
        [HttpPost("report/member/read")]
        public ActionResult<Response> ReportMemberRead([FromBody] Criteria value)
        {
            try
            {
                List<RegisterReportModel> model = new List<RegisterReportModel>(); // model report

                //filter register
                var col = new Database().MongoClient<Register>("register");
                var filter = (Builders<Register>.Filter.Ne("status", "D")) & Builders<Register>.Filter.Ne(x => x.category, "");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")) | (filter & Builders<Register>.Filter.Regex("firstName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Register>.Filter.Regex("lastName", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.status))
                    {
                        if (value.status == "VR")
                        {
                            filter = (Builders<Register>.Filter.Eq("status", "V") | Builders<Register>.Filter.Eq("status", "R"));
                        }
                        else
                        {
                            filter &= Builders<Register>.Filter.Eq("status", value.status);
                        }
                    }

                    if (!string.IsNullOrEmpty(value.username)) { filter = filter & Builders<Register>.Filter.Regex("username", new BsonRegularExpression(string.Format(".*{0}.*", value.username), "i")); }

                    var sv = new AES();
                    var encryptPassword = "";
                    if (!string.IsNullOrEmpty(value.password))
                        encryptPassword = sv.AesEncryptECB(value.password, "p3s6v8y/B?E(H+Mb");

                    if (!string.IsNullOrEmpty(value.password)) { filter = filter & Builders<Register>.Filter.Eq("password", encryptPassword); }
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Register>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Register>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<Register>.Filter.Regex("category", value.category); }
                    if (!string.IsNullOrEmpty(value.sex)) { filter = filter & Builders<Register>.Filter.Regex("sex", value.sex); }
                    if (!string.IsNullOrEmpty(value.lineID)) { filter = filter & Builders<Register>.Filter.Regex("lineID", value.lineID); }
                    if (!string.IsNullOrEmpty(value.line)) { filter = filter & Builders<Register>.Filter.Regex("line", value.line); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Register>.Filter.Regex("createBy", new BsonRegularExpression(string.Format(".*{0}.*", value.createBy), "i")); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", ds.start) & Builders<Register>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Register>.Filter.Gt("docDate", de.start) & Builders<Register>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                // by values
                List<Register> docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).ThenByDescending(c => c.username).Project(c => new Register
                {
                    code = c.code,
                    username = c.username,
                    firstName = c.firstName,
                    lastName = c.lastName,
                    password = c.password,
                    category = c.category,
                    imageUrl = c.imageUrl,
                    countUnit = c.countUnit,
                    updateDate = c.updateDate,
                    updateTime = c.updateTime,
                    createDate = c.createDate,
                    createTime = c.createTime,
                    platform = c.platform == "" ? "-" : c.platform,
                }).ToList();


                //get organization
                var cpCol = new Database().MongoClient<News>("organization");
                var cpFilter = Builders<News>.Filter.Eq("isActive", true);
                var organization = cpCol.Find(cpFilter).ToList();

                var order = 1;
                var currentUser = "";
                //docs.ForEach(c =>
                foreach(var c in docs)
                {
                    currentUser = c.username;
                    if (!string.IsNullOrEmpty(c.countUnit) && c.countUnit != "{}" && c.countUnit != "[]")
                    {
                        // model count unit
                        List<CountUnit> listCountUnit = new List<CountUnit>();

                        listCountUnit = JsonConvert.DeserializeObject<List<CountUnit>>(c.countUnit).Where(c => c.status != "D").ToList();

                        var listUnit = listCountUnit.GroupBy(g => new 
                        {
                            status = g.status,
                            lv0 = g.lv0,
                            lv1 = g.lv1,
                            lv2 = g.lv2,
                            lv3 = g.lv3,
                            lv4 = g.lv4,
                            lv5 = g.lv5,
                            titleLv0 = g.titleLv0,
                            titleLv1 = g.titleLv1,
                            titleLv2 = g.titleLv2,
                            titleLv3 = g.titleLv3,
                            titleLv4 = g.titleLv4,
                            titleLv5 = g.titleLv5
                        }).ToList();

                        if (listUnit.Count() > 0)
                        {
                            listUnit.ForEach(o =>
                            {

                            //o.titleLv2
                            //var cpCol = new Database().MongoClient("organization");
                            //var cpFilter = Builders<BsonDocument>.Filter.Eq("code", o.lv2);
                            //var cpDoc = cpCol.Find(cpFilter).FirstOrDefault();
                            var cpDoc = organization.FirstOrDefault(c => c.code == o.Key.lv2);

                                model.Add(new RegisterReportModel
                                {
                                    order = order,
                                    code = c.code,
                                    username = c.username,
                                    firstName = c.firstName,
                                    lastName = c.lastName,
                                    category = c.category,
                                    createDate = c.createDate,
                                    createTime = c.createTime,
                                    statusTitle = o.Key.status == "A" ? "ยืนยันตัวตนแล้ว" : o.Key.status == "V" || o.Key.status == "R" ? "รอการตรวจสอบ" : "รอการยืนยันตัวตน",
                                    lv0 = o.Key.lv0,
                                    lv1 = o.Key.lv1,
                                    lv2 = o.Key.lv2,
                                    lv3 = o.Key.lv3,
                                    lv4 = o.Key.lv4,
                                    lv5 = o.Key.lv5,
                                    status = o.Key.status,
                                    titleLv0 = o.Key.titleLv0 != "" ? o.Key.titleLv0 : "รอการยืนยันตัวตน",
                                    titleLv1 = o.Key.titleLv1,
                                    titleLv2 = o.Key.titleLv2,
                                    titleLv3 = o.Key.titleLv3,
                                    titleLv4 = o.Key.titleLv4,
                                    titleLv5 = o.Key.titleLv5,
                                    cpTitle = cpDoc != null ? cpDoc.cpTitle?.ToString() : ""
                                });
                            });
                            order++;
                        }
                        else
                        {
                            model.Add(new RegisterReportModel
                            {
                                order = order,
                                code = c.code,
                                username = c.username,
                                firstName = c.firstName,
                                lastName = c.lastName,
                                category = c.category,
                                createDate = c.createDate,
                                createTime = c.createTime,
                                statusTitle = "รอการยืนยันตัวตน",
                                lv0 = "",
                                lv1 = "",
                                lv2 = "",
                                lv3 = "",
                                lv4 = "",
                                lv5 = "",
                                status = c.status,
                                titleLv0 = "",
                                titleLv1 = "",
                                titleLv2 = "",
                                titleLv3 = "",
                                titleLv4 = "",
                                titleLv5 = "",
                                cpTitle = ""
                            });
                            order++;
                        }
                    }
                    else
                    {
                        model.Add(new RegisterReportModel
                        {
                            order = order,
                            code = c.code,
                            username = c.username,
                            firstName = c.firstName,
                            lastName = c.lastName,
                            category = c.category,
                            createDate = c.createDate,
                            createTime = c.createTime,
                            statusTitle = "รอการยืนยันตัวตน",
                            lv0 = "",
                            lv1 = "",
                            lv2 = "",
                            lv3 = "",
                            lv4 = "",
                            lv5 = "",
                            status = c.status,
                            titleLv0 = "",
                            titleLv1 = "",
                            titleLv2 = "",
                            titleLv3 = "",
                            titleLv4 = "",
                            titleLv5 = "",
                            cpTitle = ""
                        });
                        order++;
                    }
                };
                //);
                var count = model.Count();

                var result = model;

                return new Response { status = "S", message = "success", jsonData = result.ToJson(), objectData = result, totalData = count };

            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update Status
        [HttpGet("updateStatus")]
        public ActionResult<Response> updateStatus()
        {
            try
            {
                var col = new Database().MongoClient("register");
                var colregister = new Database().MongoClient<Register>("register");
                var filterStatus = Builders<Register>.Filter.Ne("category", "");
                var docs = colregister.Find(filterStatus).ToList();
                foreach (var c in docs)
                {
                    if (!string.IsNullOrEmpty(c.countUnit) && c.countUnit != "{}" && c.countUnit != "[]")
                    {
                        List<CountUnit> listCountUnit = new List<CountUnit>();

                        listCountUnit = JsonConvert.DeserializeObject<List<CountUnit>>(c.countUnit).Where(c => c.status != "D").ToList();

                        foreach (var cu in listCountUnit)
                            if (cu.titleLv0 == "สัตวแพทยสภา")
                                cu.titleLv0 = "สัตวแพทยสภา ชั้น 1";

                        var jsonString = JsonConvert.SerializeObject(listCountUnit);
                        var filter = Builders<BsonDocument>.Filter.Eq("code", c.code);
                        var update = Builders<BsonDocument>.Update.Set("countUnit", jsonString)
                            .Set("status", listCountUnit.Count() > 0 ? "A" : "N")
                            .Set("updateBy", "admin")
                            .Set("updateDate", DateTime.Now.toStringFromDate())
                            .Set("updateTime", DateTime.Now.toTimeStringFromDate());
                        col.UpdateOne(filter, update);
                    }
                    else
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("code", c.code);
                        var update = Builders<BsonDocument>.Update.Set("status", "N").Set("updateBy", "admin").Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                        col.UpdateOne(filter, update);
                    }

                }

                //var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                //var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                //col.UpdateOne(filter, update);
                return new Response { status = "S", message = $"code: is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        public class RegisterReportModel
        {
            public RegisterReportModel()
            {
                order = 0;
                code = "";
                username = "";
                firstName = "";
                lastName = "";
                category = "";
                createDate = "";
                createTime = "";
                statusTitle = "";
                lv0 = "";
                lv1 = "";
                lv2 = "";
                lv3 = "";
                lv4 = "";
                lv5 = "";
                status = "N";
                titleLv0 = "";
                titleLv1 = "";
                titleLv2 = "";
                titleLv3 = "";
                titleLv4 = "";
                titleLv5 = "";
                cpTitle = "";
            }

            public int order { get; set; }
            public string code { get; set; }
            public string username { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string category { get; set; }
            public string createDate { get; set; }
            public string createTime { get; set; }
            public string statusTitle { get; set; }
            public string lv0 { get; set; }
            public string lv1 { get; set; }
            public string lv2 { get; set; }
            public string lv3 { get; set; }
            public string lv4 { get; set; }
            public string lv5 { get; set; }
            public string status { get; set; }
            public string titleLv0 { get; set; }
            public string titleLv1 { get; set; }
            public string titleLv2 { get; set; }
            public string titleLv3 { get; set; }
            public string titleLv4 { get; set; }
            public string titleLv5 { get; set; }
            public string cpTitle { get; set; }
        }

        #endregion
    }
}