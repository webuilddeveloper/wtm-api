using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class PartnerController : Controller
    {
        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Partner>("partner");
                var filter = Builders<Partner>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Partner>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Partner>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (value.mainPage) { filter &= Builders<Partner>.Filter.Eq("mainPage", true); }
                    if (value.newsPage) { filter &= Builders<Partner>.Filter.Eq("newsPage", true); }
                    if (value.eventPage) { filter &= Builders<Partner>.Filter.Eq("eventPage", true); }
                    if (value.imageEventPage) { filter &= Builders<Partner>.Filter.Eq("imageEventPage", true); }
                    if (value.knowledgePage) { filter &= Builders<Partner>.Filter.Eq("knowledgePage", true); }
                    if (value.lawPage) { filter &= Builders<Partner>.Filter.Eq("lawPage", true); }
                    if (value.personnelPage) { filter &= Builders<Partner>.Filter.Eq("personnelPage", true); }
                    if (value.contactPage) { filter &= Builders<Partner>.Filter.Eq("contactPage", true); }
                    if (value.importantPage) { filter &= Builders<Partner>.Filter.Eq("importantPage", true); }
                    if (value.knowledgeVetPage) { filter &= Builders<Partner>.Filter.Eq("knowledgeVetPage", true); }
                    if (value.vetEnewsPage) { filter &= Builders<Partner>.Filter.Eq("vetEnewsPage", true); }
                    if (value.expertBranchPage) { filter &= Builders<Partner>.Filter.Eq("expertBranchPage", true); }
                    if (value.verifyApprovedUserPage) { filter &= Builders<Partner>.Filter.Eq("verifyApprovedUserPage", true); }
                    if (value.trainingInstitutePage) { filter &= Builders<Partner>.Filter.Eq("trainingInstitutePage", true); }

                    if (!string.IsNullOrEmpty(value.page)) { filter &= Builders<Partner>.Filter.Eq("isActive", true); }
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<Partner>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter &= Builders<Partner>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter &= Builders<Partner>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<Partner>.Filter.Eq("sequence", sequence); }
                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Partner>.Filter.Gt("docDate", ds.start) & Builders<Partner>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Partner>.Filter.Gt("docDate", ds.start) & Builders<Partner>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Partner>.Filter.Gt("docDate", de.start) & Builders<Partner>.Filter.Lt("docDate", de.end); }
                }
                var docs = col.Find(filter).SortBy(o => o.sequence).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.createDate, c.createBy, c.updateDate, c.updateBy, c.docDate, c.docTime, c.isActive, c.sequence, c.title, c.titleEN, c.imageUrl, c.linkUrl, c.description, c.imageUrlCreateBy, c.descriptionEN, c.action, c.note, c.contactPage, c.mainPage, c.reporterPage, c.isPostHeader, c.imageSize }).ToList();

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }
}