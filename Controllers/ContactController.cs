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
    public class ContactController : Controller
    {
        public ContactController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] Contact value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("contact");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "title", value.title },
                    { "sequence", value.sequence },
                    { "imageUrl", value.imageUrl },
                    { "category", value.category },
                    { "language", value.language },
                    { "phone", value.phone },
                    { "note", value.note },
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
                    { "status", value.isActive ? "A": "N" },
                    { "lv0", value.lv0 },
                    { "lv1", value.lv1 },
                    { "lv2", value.lv2 },
                    { "lv3", value.lv3 },
                    { "lv4", value.lv4 },
                    { "isPublic", value.isPublic },
                    { "isHighlight", value.isHighlight },
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
                var col = new Database().MongoClient<Contact>("contact");
                var filter = (Builders<Contact>.Filter.Ne("status", "D") & value.filterOrganization<Contact>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Contact>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<Contact>("category");

                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Contact>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= value.permission.filterPermission<Contact>("category");


                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Contact>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Contact>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Contact>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Contact>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Contact>.Filter.Eq("sequence", sequence); }
                }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).SortByDescending(o => o.docDate).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.titleEN, c.imageUrl, c.title, c.updateBy, c.updateDate, c.sequence, c.category, c.phone, c.note, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();

                List<Contact> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("contactCategory", "category", "code", "categoryList")
                                      .As<Contact>()
                                      .ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
            
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] Contact value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("contact");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);

                if (value.organizationMode == "auto")
                {
                    var og = value.organization.filterQrganizationAuto();
                    value.lv0 = og.lv0;
                    value.lv1 = og.lv1;
                    value.lv2 = og.lv2;
                    value.lv3 = og.lv3;
                    value.lv4 = og.lv4;
                }

                

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                doc["phone"] = value.phone;
                doc["note"] = value.note;

                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["isPublic"] = value.isPublic;
                doc["isHighlight"] = value.isHighlight;
                doc["status"] = value.isActive ? "A" : "N";
                doc["lv0"] = value.lv0;
                doc["lv1"] = value.lv1;
                doc["lv2"] = value.lv2;
                doc["lv3"] = value.lv3;
                doc["lv4"] = value.lv4;
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
                var col = new Database().MongoClient( "contact");

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
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "contactGallery");

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
                    { "createDate", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "reference", value.reference }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            try
            {
                var doc = new BsonDocument();
                var col = new Database().MongoClient( "contactGallery");

                var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false);
                col.UpdateMany(filter, update);
                return new Response { status = "S", message = $"code: {value.code} is delete" };
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
        public ActionResult<Response> CategoryCreate([FromBody] Category value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "contactCategory");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "sequence", value.sequence },
                    { "language", value.language },
                    { "title", value.title },
                    { "imageUrl", value.imageUrl },
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
                var col = new Database().MongoClient<Category>( "contactCategory");

                var filter = Builders<Category>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<Category>("code");

                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Category>.Filter.Eq("title", value.category);
                    else
                        if (value.permission != "all")
                            filter &= value.permission.filterPermission<Category>("code");


                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<ContactCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<ContactCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.updateDate, c.updateBy, c.sequence }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> CategoryUpdate([FromBody] Category value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "contactCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                doc["sequence"] = value.sequence;
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                col.ReplaceOne(filter, doc);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("contact");
                    var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "N");
                    collectionContent.UpdateMany(filterContent, updateContent);
                }
                // ------- end ------

                // ------- update register permission ------
                if (!value.isActive)
                {
                    var collectionPermission = new Database().MongoClient("registerPermission");
                    var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updatePermission = Builders<BsonDocument>.Update.Set("contactPage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------

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
                var col = new Database().MongoClient( "contactCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("contact");
                    var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "D");
                    collectionContent.UpdateMany(filterContent, updateContent);
                }
                // ------- end ------

                // ------- update register permission ------
                if (!value.isActive)
                {
                    var collectionPermission = new Database().MongoClient("registerPermission");
                    var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updatePermission = Builders<BsonDocument>.Update.Set("contactPage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------
                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region re order

        // POST /create
        [HttpPost("reorder")]
        public ActionResult<Response> ReOrder([FromBody] EventCalendar value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient<BsonDocument>("contact");

                var codeList = value.code.Split(",");

                int sequence = 10;
                foreach (var code in codeList)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("sequence", sequence).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);
                    //sequence++;
                }


                return new Response { status = "S", message = "success", jsonData = value.ToJson(), objectData = value };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /create
        [HttpPost("reorderAll")]
        public ActionResult<Response> ReOrderAll([FromBody] EventCalendar value)
        {

            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient<BsonDocument>("contact");
                var arrayFilter = Builders<BsonDocument>.Filter.Ne("sequence", 10) & Builders<BsonDocument>.Filter.Ne("status", "D");
                var arrayUpdate = Builders<BsonDocument>.Update.Set("sequence", 10).Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());

                col.UpdateMany(arrayFilter, arrayUpdate);

                return new Response { status = "S", message = "success", jsonData = value.ToJson(), objectData = value };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region report
        // POST /read
        [HttpPost("report/read")]
        public ActionResult<Response> ReportRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<Contact>("contact");
                var filter = (Builders<Contact>.Filter.Ne("status", "D") & value.filterOrganization<Contact>());

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Contact>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= (value.permission.filterPermission<Contact>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Contact>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= (value.permission.filterPermission<Contact>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Contact>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<Contact>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<Contact>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Contact>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Contact>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Contact>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Contact>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Contact>.Filter.Gt("docDate", ds.start) & Builders<Contact>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Contact>.Filter.Gt("docDate", ds.start) & Builders<Contact>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Contact>.Filter.Gt("docDate", de.start) & Builders<Contact>.Filter.Lt("docDate", de.end); }
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Project(c => new { c.code, c.title, c.category, c.isActive, c.createBy, c.createDate, c.updateBy, c.updateDate, c.status }).ToList();

                #region get master
                // get category.
                var colCategory = new Database().MongoClient<Contact>("contactCategory");
                var filterCategory = Builders<Contact>.Filter.Eq("isActive", true);
                var newsCategory = colCategory.Find(filterCategory).ToList();

                // get name user role.
                var colRole = new Database().MongoClient<registerRole>("registerRole");
                var filterRole = Builders<registerRole>.Filter.Eq(x => x.isActive, true);
                //if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", c.username); }
                var registerRole = colRole.Find(filterRole).Project(c => new { c.username, c.category }).ToList();

                // get register category receive lv organize.
                var colRegisterCat = new Database().MongoClient<RegisterCategory>("registerCategory");
                var filterRegisterCat = Builders<RegisterCategory>.Filter.Eq(x => x.isActive, true);
                var registerCategory = colRegisterCat.Find(filterRegisterCat).Project(c => new { c.title, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5 }).ToList();

                //get Organization
                var colOrganiztion = new Database().MongoClient<Contact>("organization");
                var filterOrganiztion = Builders<Contact>.Filter.Ne("status", "D");
                var docOraganiztion = colOrganiztion.Find(filterOrganiztion).Project(c => new { c.code, c.title, c.lv0, c.lv1, c.lv2, c.lv3, c.lv4, c.lv5, c.cpCode, c.cpTitle }).ToList();
                #endregion

                var result = new List<object>();

                docs.ForEach(c => {
                    try
                    {
                        int order = 1;
                        // get category.
                        var titleCategory = newsCategory.FirstOrDefault(o => o.code == c.category)?.title;

                        // get name user role.
                        var docsRole = registerRole.FirstOrDefault(f => f.username == c.createBy);

                        // get register category receive lv organize.
                        var docsRegisterCat = registerCategory.FirstOrDefault(f => f.title == docsRole.category);

                        var listLv0 = !string.IsNullOrEmpty(docsRegisterCat.lv0) ? docsRegisterCat.lv0.Split(",") : new string[0];
                        var listLv1 = !string.IsNullOrEmpty(docsRegisterCat.lv1) ? docsRegisterCat.lv1.Split(",") : new string[0];
                        var listLv2 = !string.IsNullOrEmpty(docsRegisterCat.lv2) ? docsRegisterCat.lv2.Split(",") : new string[0];
                        var listLv3 = !string.IsNullOrEmpty(docsRegisterCat.lv3) ? docsRegisterCat.lv3.Split(",") : new string[0];
                        var listLv4 = !string.IsNullOrEmpty(docsRegisterCat.lv4) ? docsRegisterCat.lv4.Split(",") : new string[0];
                        var listLv5 = !string.IsNullOrEmpty(docsRegisterCat.lv5) ? docsRegisterCat.lv5.Split(",") : new string[0];

                        String lv0 = "";
                        String lv1 = "";
                        String lv2 = "";
                        String lv3 = "";
                        String lv4 = "";
                        String lv5 = "";
                        String titleLv0 = "";
                        String titleLv1 = "";
                        String titleLv2 = "";
                        String titleLv3 = "";
                        String titleLv4 = "";
                        String titleLv5 = "";
                        String cpCode = "";
                        String cpTitle = "";

                        if (listLv0.Length > 0)
                        {
                            lv0 = listLv0[0];
                            titleLv0 = docOraganiztion.FirstOrDefault(c => c.code == listLv0[0])?.title;
                        }
                        if (listLv1.Length > 0)
                        {
                            lv1 = listLv1[0];
                            titleLv1 = docOraganiztion.FirstOrDefault(c => c.code == listLv1[0] && c.lv0 == listLv0[0])?.title;
                        }
                        if (listLv2.Length > 0)
                        {
                            lv2 = listLv2[0];
                            titleLv2 = docOraganiztion.FirstOrDefault(c => c.code == listLv2[0] && c.lv1 == listLv1[0])?.title;
                            cpCode = docOraganiztion.FirstOrDefault(c => c.code == listLv2[0] && c.lv1 == listLv1[0])?.cpCode;
                            cpTitle = docOraganiztion.FirstOrDefault(c => c.code == listLv2[0] && c.lv1 == listLv1[0])?.cpTitle;
                        }
                        if (listLv3.Length > 0)
                        {
                            lv3 = listLv3[0];
                            titleLv3 = docOraganiztion.FirstOrDefault(c => c.code == listLv3[0] && c.lv2 == listLv2[0])?.title;
                        }
                        if (listLv4.Length > 0)
                        {
                            lv4 = listLv4[0];
                            titleLv4 = docOraganiztion.FirstOrDefault(c => c.code == listLv4[0] && c.lv3 == listLv3[0])?.title;
                        }
                        if (listLv5.Length > 0)
                        {
                            lv5 = listLv5[0];
                            titleLv5 = docOraganiztion.FirstOrDefault(c => c.code == listLv5[0] && c.lv4 == listLv4[0])?.title;
                        }

                        result.Add(new
                        {
                            order = order,
                            title = c.title,
                            category = titleCategory,
                            createBy = c.createBy,
                            createDate = c.createDate,
                            updateBy = c.updateBy,
                            updateDate = c.updateDate,
                            status = c.status,
                            isActive = c.isActive,
                            lv0 = lv0,
                            lv1 = lv1,
                            lv2 = lv2,
                            lv3 = lv3,
                            lv4 = lv4,
                            lv5 = lv5,
                            titleLv0 = titleLv0,
                            titleLv1 = titleLv1,
                            titleLv2 = titleLv2,
                            titleLv3 = titleLv3,
                            titleLv4 = titleLv4,
                            titleLv5 = titleLv5,
                            cpTitle = cpTitle,
                            cpCode = cpCode,
                        });
                        order++;
                    }
                    catch { }
                });



                return new Response { status = "S", message = "success", objectData = result, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
        #endregion
    }
}