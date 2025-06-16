using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        public IpController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] User value)
        {

            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("_logIP");

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
                    { "ipAddress", value.ipAddress },
                    { "page", value.page },
                    { "username", value.username },
                    { "createBy", value.username },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
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
                var col = new Database().MongoClient<User>("_logIP");

                var filter = Builders<User>.Filter.Ne(x => x.code, "");
                if (value.status2 == "guest")
                {
                    filter &= Builders<User>.Filter.Eq("username", "");

                }
                if (value.status2 == "member")
                {
                    filter &= Builders<User>.Filter.Ne("username", "");

                }

                List<User> data = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).SortByDescending(o => o.docDate).Project(c => new User
                {
                    code = c.code,
                    ipAddress = c.ipAddress,
                    page = c.page,
                    username = c.username,
                    createBy = c.createBy,
                    createDate = c.createDate,
                    docDate = c.docDate,
                    docTime = c.docTime,

                }).ToList();

                if (value.status2 == "guest")
                {
                    var result = data.GroupBy(g => new { g.ipAddress, g.page, g.docDate.Date }).Select(c => new {
                        username = c.OrderByDescending(o => o.ipAddress).FirstOrDefault().username,
                        page = c.Key.page,
                        ipAddress = c.Key.ipAddress,
                        count = c.ToList().Count(),
                        docDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().docDate,
                        docTime = c.OrderByDescending(o => o.docDate).FirstOrDefault().docTime,
                        createBy = c.OrderByDescending(o => o.docDate).FirstOrDefault().createBy,
                        createDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().createDate,
                    }).ToList();

                    return new Response { status = "S", message = "success", objectData = result.Skip(value.skip).Take(value.limit).ToList(), totalData = result.Count() };
                }
                if (value.status2 == "member")
                {
                    var result = data.GroupBy(g => new { g.username, g.page, g.docDate.Date }).Select(c => new {
                        username = c.Key.username,
                        page = c.Key.page,
                        ipAddress = c.OrderByDescending(o => o.ipAddress).FirstOrDefault().ipAddress,
                        count = c.ToList().Count(),
                        docDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().docDate,
                        docTime = c.OrderByDescending(o => o.docDate).FirstOrDefault().docTime,
                        createBy = c.OrderByDescending(o => o.docDate).FirstOrDefault().createBy,
                        createDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().createDate,
                    }).ToList();

                    return new Response { status = "S", message = "success", objectData = result.Skip(value.skip).Take(value.limit).ToList(), totalData = result.Count() };
                }

                var docs = new List<object>();
                data.Skip(value.skip).Take(value.limit).ToList().ForEach(c =>
                {
                    docs.Add(new
                    {
                        c.ipAddress,
                        c.page,
                        count = 1,
                        c.username,
                        c.createBy,
                        c.createDate,
                        c.docDate,
                        c.docTime,
                    });
                });

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
        #endregion

        #region mainCMS

        // POST /create
        [HttpPost("createCms")]
        public ActionResult<Response> CreateCms([FromBody] User value)
        {

            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("_logIPCms");

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
                    { "ipAddress", value.ipAddress },
                    { "page", value.page },
                    { "username", value.username },
                    { "createBy", value.username },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "docDate", DateTime.Now },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
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
        [HttpPost("readCms")]
        public ActionResult<Response> ReadCms([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<User>("_logIPCms");

                var filter = Builders<User>.Filter.Ne(x => x.code, "");

                if (value.status2 == "guest")
                {
                    filter &= Builders<User>.Filter.Eq("username", "");

                }
                if (value.status2 == "member")
                {
                    filter &= Builders<User>.Filter.Ne("username", "");

                }

                var data = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).SortByDescending(o => o.docDate).Project(c => new {
                    c.code,
                    c.ipAddress,
                    c.page,
                    c.username,
                    c.createBy,
                    c.createDate,
                    c.docDate,
                    c.docTime,
                }).ToList();

                var result = data.GroupBy(g => new { g.username, g.page, g.docDate.Date }).Select(c => new {
                    username = c.Key.username,
                    page = c.Key.page,
                    ipAddress = c.OrderByDescending(o => o.ipAddress).FirstOrDefault().ipAddress,
                    count = c.ToList().Count(),
                    docDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().docDate,
                    docTime = c.OrderByDescending(o => o.docDate).FirstOrDefault().docTime,
                    createBy = c.OrderByDescending(o => o.docDate).FirstOrDefault().createBy,
                    createDate = c.OrderByDescending(o => o.docDate).FirstOrDefault().createDate,
                }).ToList();

                return new Response { status = "S", message = "success", objectData = result.Skip(value.skip).Take(value.limit).ToList(), totalData = result.Count() };

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }
        #endregion

        // POST /read
        [HttpGet("readCount")]
        public ActionResult<Response> ReadCount()
        {
            try
            {
                var col = new Database().MongoClient<User>("_logIP");


                //var docs = col.Aggregate().Match(_ => true).Project(c => new
                //{
                //    c.code,
                //    c.ipAddress,
                //    c.page,
                //    c.username,
                //    c.createBy,
                //    c.createDate,
                //    c.docDate,
                //    c.docTime,
                //}).ToList();

                DateTime now = DateTime.Now;

                var yearStart = new DateTime(now.Year, 1, 1);
                var yearEnd = new DateTime(now.Year, 12, 1);
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var weekago2 = now.AddDays(-6);
                var weekago = now.Day;
                int[] week1 = { 1, 2, 3, 4, 5, 6, 7 };
                int[] week2 = { 8, 9, 10, 11, 12, 13, 14 };
                int[] week3 = { 15, 16, 17, 18, 19, 20, 21 };


                var strat = new DateTime(now.Year, now.Month, 22);
                var end = monthStart.AddMonths(1).AddDays(-1);
                //var countWeek = "0";
                if (week1.Any(c => c == weekago))
                {
                    strat = new DateTime(now.Year, now.Month, 1);
                    end = new DateTime(now.Year, now.Month, 7);
                    //countWeek = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= strat && c.docDate <= end).ToList().Count().ToString("#,###");
                }
                if (week2.Any(c => c == weekago))
                {
                    strat = new DateTime(now.Year, now.Month, 8);
                    end = new DateTime(now.Year, now.Month, 14);
                    //countWeek = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= strat && c.docDate <= end).ToList().Count().ToString("#,###");
                }
                if (week3.Any(c => c == weekago))
                {
                    strat = new DateTime(now.Year, now.Month, 15);
                    end = new DateTime(now.Year, now.Month, 21);
                    //countWeek = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= strat && c.docDate <= end).ToList().Count().ToString("#,###");
                }

                var filter = Builders<User>.Filter.Where(x => x.page.ToUpper() == "HOME");
                var result = new
                {
                    // จำนวนครั้ง
                    //countNU = docs.Where(c => string.IsNullOrEmpty(c.username) && c.page.ToUpper() == "HOME").GroupBy(g => g.page).Select(s => s.Count().ToString("#,###")),
                    //countU = docs.Where(c => !string.IsNullOrEmpty(c.username) && c.page.ToUpper() == "HOME").GroupBy(g => g.page).Select(s => s.Count().ToString("#,###")),
                    //countYear = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= yearStart && c.docDate <= yearEnd).ToList().Count().ToString("#,###"),
                    //countMonth = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= monthStart && c.docDate <= monthEnd).ToList().Count().ToString("#,###"),
                    //countWeek = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= weekago && c.docDate <= now).ToList().Count().ToString("#,###"),
                    //countDay = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= now.Date && c.docDate <= now).ToList().Count().ToString("#,###"),

                    countNU = col.CountDocuments(filter & Builders<User>.Filter.Eq("username", "")),
                    countU = col.CountDocuments(filter & Builders<User>.Filter.Ne("username", "")),
                    countYear = col.CountDocuments(filter & Builders<User>.Filter.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= yearStart && c.docDate <= yearEnd)),
                    countMonth = col.CountDocuments(filter & Builders<User>.Filter.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= monthStart && c.docDate <= monthEnd)),
                    countWeek = col.CountDocuments(filter & Builders<User>.Filter.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= strat && c.docDate <= end)),
                    countDay = col.CountDocuments(filter & Builders<User>.Filter.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= now.Date && c.docDate <= now)),

                    // จำนวนคน
                    //countYear = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= yearStart && c.docDate <= yearEnd).GroupBy(g => new { g.ipAddress, g.docDate.Date }).ToList().Count().ToString("#,###"),
                    //countMonth = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= monthStart && c.docDate <= monthEnd).GroupBy(g => new { g.ipAddress, g.docDate.Date }).ToList().Count().ToString("#,###"),
                    //countWeek = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= weekago && c.docDate <= now.AddDays(-1)).GroupBy(g => new { g.ipAddress, g.docDate.Date }).ToList().Count().ToString("#,###"),
                    //countDay = docs.Where(c => c.page.ToUpper() == "HOME" && c.docDate >= now.Date && c.docDate <= now).ToList().Count().ToString("#,###"),
                };

                return new Response { status = "S", message = "success", objectData = result };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("createAcceptPolicy")]
        public ActionResult<Response> CreateAcceptPolicy([FromBody] User value)
        {

            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("_logIPAcceptPolicy");

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
                    { "ipAddress", value.ipAddress },
                    { "page", value.page },
                    { "username", value.username },
                    { "createBy", value.username },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

    }
}
