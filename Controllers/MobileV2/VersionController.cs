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

namespace mobilev2_api.Controllers
{
    [Route("m/v2/[controller]")]
    public class VersionController : Controller
    {
        public VersionController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<object> Read([FromBody] Version param)
        {
            try
            {
                var col = new Database().MongoClient<Version>(Collection.configVersion);
                var query = col.AsQueryable().Where(c => !c.isDelete);
                if (!string.IsNullOrEmpty(param.platform)) query = query.Where(c => c.platform.Contains(param.platform));

                var model = query.FirstOrDefault();
                if (model != null)
                {
                    var obj = new
                    {
                        version = Convert.ToInt16(model.version.Replace(".", "")),
                        isForce = model.isForce,
                        title = model.title ?? "",
                        description = model.description ?? "",
                        url = model.url ?? "https://google.com",
                        isActive = model.isActive
                    };
                    return new { status = "S", message = "success", objectData = obj };
                }
                else
                {
                    var obj = new
                    {
                        version = 0,
                        isForce = false,
                        title = "",
                        description = "",
                        url = "",
                        isActive = false
                    };

                    return new { status = "S", message = "success", objectData = obj };
                }
            }
            catch (Exception ex)
            {
                return new { status = "E", message = ex.Message };
            }
        }
    }

}
