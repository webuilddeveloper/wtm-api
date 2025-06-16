using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Version = cms_api.Models.Version;

namespace configulation_api.Controllers
{
    [Route("[controller]")]
    public class VersionController : Controller
    {
        public VersionController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<object> Create([FromBody] Version param)
        {
            try
            {
                var col = new Database().MongoClient<Version>(Collection.configVersion);
                col.InsertOne(param);
                return new { status = "S", message = "success", objectData = param };
            }
            catch (Exception ex)
            {
                return new { status = "E", message = ex.Message, objectData = param };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<object> Read([FromBody] Version param)
        {
            try
            {
                var col = new Database().MongoClient<Version>(Collection.configVersion);
                var query = col.AsQueryable().Where(c => !c.isDelete);
                if (!string.IsNullOrEmpty(param.platform)) query = query.Where(c => c.platform.Contains(param.platform));

                var model = query.ToList();
                return new { status = "S", message = "success", objectData = model };

            }
            catch (Exception ex)
            {
                return new { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<object> Update([FromBody] Version param)
        {
            param.updateDate = DateTime.Now.toStringFromDate();
            param.updateTime = DateTime.Now.toTimeStringFromDate();

            try
            {
                var col = new Database().MongoClient<Version>(Collection.configVersion);
                col.ReplaceOne(c => c._id == param._id, param);

                return new { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new { status = "E", message = ex.Message };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<object> Delete([FromBody] List<Version> param)
        {
            try
            {
                var col = new Database().MongoClient<Version>(Collection.configVersion);

                param.ForEach(p =>
                {
                    col.ReplaceOne(c => c._id == p._id, p);
                });

                //foreach (var code in codeList)
                //{

                //    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                //    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                //    col.UpdateOne(filter, update);

                //}

                return new { status = "S", message = "Success" };
            }
            catch (Exception ex)
            {
                return new { status = "E", message = ex.Message };
            }
        }
    }

    #endregion
}
