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
    public class DashboardController : Controller
    {
        public DashboardController() { }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {

            try
            {
                var col = new Database().MongoClient<Veterinary2>("m_Veterinary_2");
                var filter = Builders<Veterinary2>.Filter.Ne("status", "D");

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", ds.start) & Builders<Veterinary2>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", ds.start) & Builders<Veterinary2>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", de.start) & Builders<Veterinary2>.Filter.Lt("docDate", de.end); }

                var docs = new
                {
                    news = col.CountDocuments(filter & Builders<Veterinary2>.Filter.Eq("category", "")),
                    old = col.CountDocuments(filter & Builders<Veterinary2>.Filter.Ne("category", "")),
                };

                return new Response { status = "S", message = "success", objectData = docs, totalData = docs.news + docs.old };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
            
        }

        #endregion

    }
}