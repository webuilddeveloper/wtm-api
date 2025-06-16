using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class EventCalendarController : Controller
    {
        public EventCalendarController() { }

        public async Task<Response2> readWebAsync(Criteria value, bool isSkip = false)
        {
            var datajson = new
            {
                permission = "all",
                profileCode = value.profileCode,
                skip = isSkip ? 0 : value.skip,
                limit = value.limit,
                keySearch = value.keySearch,
            };
       
            HttpClient client = new HttpClient();
            var json = JsonConvert.SerializeObject(datajson);
            HttpContent httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync("https://vetweb.we-builds.com/vet-api/m/eventCalendar/readMain", httpContent);
            //var response = await client.PostAsync("http://localhost:5300/m/eventCalendar/readMain", httpContent);
            var responseString = await response.Content.ReadAsStringAsync();
            Response2 model = Newtonsoft.Json.JsonConvert.DeserializeObject<Response2>(responseString);


            return model;
        }

        // POST /read
        [HttpPost("read")]
        public async Task<ActionResult<Response>> ReadAsync([FromBody] Criteria value)
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                var dataWeb = new List<obj>();
                if (value.category == "20220420131310-872-122")
                {

                    var responseWeb = await readWebAsync(value);
                    return new Response { status = responseWeb.status, message = responseWeb.message, objectData = responseWeb.objectData, totalData = responseWeb.totalData };
                }
                else
                {

                    //value.statisticsCreateAsync("eventCalendar");
                    var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                    var filter = Builders<EventCalendar>.Filter.Eq("status", "A");
                    filter = filter & Builders<EventCalendar>.Filter.Eq("isActive", true);
                    //var filter = (Builders<EventCalendar>.Filter.Eq("status", "A"));
                    if (!string.IsNullOrEmpty(value.keySearch))
                    {
                        filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"));

                        if (value.skip == 1)
                        {

                            var responseWeb = await readWebAsync(value, true);
                            dataWeb = responseWeb.objectData;
                        }
                        //BEGIN : Statistic
                        try
                        {
                            var value1 = new Criteria();

                            value1.title = value.keySearch;
                            value1.updateBy = value.updateBy;
                            value1.platform = value.platform;

                            if (!string.IsNullOrEmpty(value.code))
                                value1.reference = value.code;

                            value1.statisticsCreateAsync("eventCalendarKeySearch");
                        }
                        catch { }
                        //END : Statistic
                    }

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<EventCalendar>.Filter.Eq("code", value.code); }
                    else if (!string.IsNullOrEmpty(value.center)) { filter = filter & Builders<EventCalendar>.Filter.Eq("center", value.center); }
                    else { filter = filter & Builders<EventCalendar>.Filter.Eq("center", ""); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<EventCalendar>.Filter.Eq("category", value.category); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description))); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                    if (value.isHighlight) { filter = filter & Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                    List<EventCalendar> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new EventCalendar
                    {
                        code = c.code,
                        isActive = c.isActive,
                        createBy = c.createBy,
                        imageUrlCreateBy = c.imageUrlCreateBy,
                        createDate = c.createDate,
                        description = c.description,
                        descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                        imageUrl = c.imageUrl,
                        title = c.title,
                        titleEN = c.titleEN != "" ? c.titleEN : c.title,
                        language = c.language,
                        updateBy = c.updateBy,
                        updateDate = c.updateDate,
                        category = c.category,
                        confirmStatus = c.confirmStatus,
                        linkFacebook = c.linkFacebook,
                        linkYoutube = c.linkYoutube,
                        dateStart = c.dateStart,
                        dateEnd = c.dateEnd,
                        view = c.view,
                        linkUrl = c.linkUrl,
                        textButton = c.textButton,
                        fileUrl = c.fileUrl,
                    }).ToList();


                    if (docs.Count == 0 && ipAddress.ToString() == "fe80::4a1:9cd6:4b07:2a59%13")
                    {
                        var datajson = new
                        {
                            permission = "all",
                            profileCode = value.profileCode,
                            skip = value.skip,
                            limit = value.limit,
                            keySearch = value.keySearch,
                            code = value.code
                        };
                        HttpClient client = new HttpClient();
                        var json = JsonConvert.SerializeObject(datajson);
                        HttpContent httpContent = new StringContent(json);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        var response = await client.PostAsync("https://vetweb.we-builds.com/vet-api/m/eventCalendar/read", httpContent);
                        //var response = await client.PostAsync("http://localhost:5300/m/eventCalendar/read", httpContent);
                        var responseString = await response.Content.ReadAsStringAsync();
                        Response2 model = Newtonsoft.Json.JsonConvert.DeserializeObject<Response2>(responseString);


                        return new Response { status = model.status, message = model.message, objectData = model.objectData, totalData = model.totalData };
                    }
                    else
                    {
                        //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                        if (!string.IsNullOrEmpty(value.code))
                        {
                            var view = docs[0].view;

                            var doc = new BsonDocument();
                            var colUpdate = new Database().MongoClient("eventCalendar");

                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                            doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                            var model = BsonSerializer.Deserialize<object>(doc);
                            doc["view"] = view + 1;
                            colUpdate.ReplaceOne(filterUpdate, doc);

                            docs[0].view += 1;

                            //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                        }
                        //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                        //BEGIN : Statistic
                        try
                        {
                            if (!string.IsNullOrEmpty(value.code))
                            {
                                //Get Category
                                var colCategory = new Database().MongoClient<Contact>("eventCalendarCategory");
                                var filterCategory = Builders<Contact>.Filter.Eq("code", docs[0].category);
                                Category docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code, title = c.title }).FirstOrDefault();

                                value.reference = value.code;
                                value.title = docs.Count > 0 ? docs[0].title : "";
                                value.category = docCategory.title;

                                value.statisticsCreateAsync("eventCalendar");
                            }

                        }
                        catch { }

                        //endStatistic

                        // get User: firstname lastname, imageurl
                        var colCenter = new Database().MongoClient<Register>("mCenter");
                        docs.ForEach(c =>
                        {
                            try
                            {
                            //Get Profile
                            var colRegister = new Database().MongoClient<Register>("register");
                                var filterRegister = Builders<Register>.Filter.Eq("username", c.createBy);
                                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                                c.createBy = docRegister.firstName + " " + docRegister.lastName;

                                var filterRegister2 = Builders<Register>.Filter.Eq("username", c.updateBy);
                                Register docRegister2 = colRegister.Find(filterRegister).Project(c => new Register { imageUrl = c.imageUrl, firstName = c.firstName, lastName = c.lastName, center = c.center }).FirstOrDefault();

                                c.imageUrlCreateBy = docRegister2.imageUrl;
                                c.updateBy = docRegister2.firstName + " " + docRegister2.lastName;

                                c.centerName = colCenter.Find(o => o.code == c.center).FirstOrDefault()?.title ?? "";
                                c.centerNameEN = colCenter.Find(o => o.code == c.center).FirstOrDefault()?.titleEN ?? "";
                            }
                            catch
                            {

                            }
                        });

                        dataWeb.ForEach(c =>
                        {
                            try
                            {
                                docs.Add(new EventCalendar
                                {
                                    code = c.code,
                                    title = c.title,
                                    titleEN = c.titleEN != "" ? c.titleEN : c.title,
                                    imageUrl = c.imageUrl,
                                    sequence = c.sequence,
                                    category = c.category,
                                    description = c.description,
                                    descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                                    createBy = c.createBy,
                                    createDate = c.createDate,
                                    createTime = c.createTime,
                                    updateBy = c.updateBy,
                                    updateDate = c.updateDate,
                                    updateTime = c.updateTime,
                                    isActive = c.isActive,
                                    status = c.status,
                                    isHighlight = c.isHighlight,
                                    isPublic = c.isPublic,
                                    isNotification = c.isNotification,
                                    docDate = c.docDate,
                                    docTime = c.docTime,
                                    center = c.center,
                                    textButton = c.textButton,
                                    linkUrl = c.linkUrl,
                                    fileUrl = c.fileUrl,
                                    view = c.view,

                                    dateStart = c.dateStart,
                                    dateEnd = c.dateEnd,
                                    linkYoutube = c.linkYoutube,
                                    linkFacebook = c.linkFacebook,
                                    confirmStatus = c.confirmStatus,

                                    imageUrlCreateBy = "",

                                });
                            }
                            catch
                            {

                            }
                        });

                        // where by date
                        var filterData = new List<object>();

                        if (!string.IsNullOrEmpty(value.date))
                        {
                            string dateSub = value.date.Substring(0, 10);
                            string[] dateArray = dateSub.Split("-");
                            string date = dateArray[0] + dateArray[1] + dateArray[2];
                            bool dateBetween = false;

                            foreach (var c in docs)
                            {
                                dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                                if (dateBetween)
                                {
                                    filterData.Add(c);
                                }
                            }

                            return new Response { status = "S", message = "success", jsonData = filterData.ToJson(), objectData = filterData };
                        }

                        return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("readMain")]
        public ActionResult<Response> readMain([FromBody] Criteria value)
        {
            try
            {
                //value.statisticsCreateAsync("eventCalendar");
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());
                //var filter = (Builders<EventCalendar>.Filter.Eq("status", "A"));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"));

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("eventCalendarKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }

                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<EventCalendar>.Filter.Eq("code", value.code); }
                //else if (!string.IsNullOrEmpty(value.center)) { filter = filter & Builders<EventCalendar>.Filter.Eq("center", value.center); }
                //else { filter = filter & Builders<EventCalendar>.Filter.Eq("center", ""); }
                if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<EventCalendar>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description))); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));


                List<EventCalendar> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new EventCalendar
                {
                    code = c.code,
                    isActive = c.isActive,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    createDate = c.createDate,
                    description = c.description,
                    descriptionEN = c.descriptionEN != "" ? c.descriptionEN : c.description,
                    imageUrl = c.imageUrl,
                    title = c.title,
                    titleEN = c.titleEN != "" ? c.titleEN : c.title,
                    language = c.language,
                    updateBy = c.updateBy,
                    updateDate = c.updateDate,
                    category = c.category,
                    confirmStatus = c.confirmStatus,
                    linkFacebook = c.linkFacebook,
                    linkYoutube = c.linkYoutube,
                    dateStart = c.dateStart,
                    dateEnd = c.dateEnd,
                    view = c.view,
                    linkUrl = c.linkUrl,
                    textButton = c.textButton,
                    fileUrl = c.fileUrl
                }).ToList();

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("eventCalendar");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs[0].view += 1;

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        //Get Category
                        var colCategory = new Database().MongoClient<Contact>("eventCalendarCategory");
                        var filterCategory = Builders<Contact>.Filter.Eq("code", docs[0].category);
                        Category docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code, title = c.title }).FirstOrDefault();

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docCategory.title;

                        value.statisticsCreateAsync("eventCalendar");
                    }

                }
                catch { }

                //endStatistic

                // get User: firstname lastname, imageurl
                docs.ForEach(c =>
                {
                    try
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Eq("username", c.createBy);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.createBy = docRegister.firstName + " " + docRegister.lastName;

                        var filterRegister2 = Builders<Register>.Filter.Eq("username", c.updateBy);
                        Register docRegister2 = colRegister.Find(filterRegister).Project(c => new Register { imageUrl = c.imageUrl, firstName = c.firstName, lastName = c.lastName, center = c.center }).FirstOrDefault();

                        c.imageUrlCreateBy = docRegister2.imageUrl;
                        c.updateBy = docRegister2.firstName + " " + docRegister2.lastName;
                    }
                    catch
                    {

                    }
                });

                // where by date
                var filterData = new List<object>();

                if (!string.IsNullOrEmpty(value.date))
                {
                    string dateSub = value.date.Substring(0, 10);
                    string[] dateArray = dateSub.Split("-");
                    string date = dateArray[0] + dateArray[1] + dateArray[2];
                    bool dateBetween = false;

                    foreach (var c in docs)
                    {
                        dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                        if (dateBetween)
                        {
                            filterData.Add(c);
                        }
                    }

                    return new Response { status = "S", message = "success", jsonData = filterData.ToJson(), objectData = filterData };
                }

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("v2/read")]
        public ActionResult<Response> V2Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A"));

                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.center)) { filter = filter & Builders<EventCalendar>.Filter.Eq("center", value.center); }
                else { filter = filter & Builders<EventCalendar>.Filter.Eq("center", ""); }
                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                //if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                //else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }

                var docsEventCalendar = col.Find(filter).Project(c => new EventCalendar
                {
                    code = c.code,
                    title = c.title,
                    imageUrl = c.imageUrl,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    description = c.description,
                    updateBy = c.updateBy,
                    category = c.category,
                    view = c.view,
                    createDate = c.createDate,
                    updateDate = c.updateDate,
                    dateStart = c.dateStart,
                    dateEnd = c.dateEnd,
                    //createDateSubstring = c.createDate.Substring(0, 8),
                    //dateStartSubstringDay = c.dateStart.Substring(6, 2),
                    //dateStartSubstringMonth = c.dateStart.Substring(4, 2),
                    //dateStartSubstringYear = c.dateStart.Substring(0, 4),
                }).ToList();

                //List<List<EventCalendar>> groupedList = docsEventCalendar.Where(c => c.dateStart.toDateFromString().toBetweenDate().start >= ds.start  && c.dateEnd.toDateFromString().toBetweenDate().start <= de.end).OrderByDescending(c => c.updateDate)
                //    .GroupBy(u => u.dateStart)
                //    .Select(grp =>  grp.ToList())
                //    .ToList();


                //List<EventCalendarGroup> items = new List<EventCalendarGroup>();
                //groupedList.ForEach(c =>
                //{
                //    items.Add(c.GroupBy(uu => uu.dateStart).Select(item => new EventCalendarGroup { date = item.Key, length = item.ToList().Count(), items = item.ToList() }).FirstOrDefault());
                //});

                var yearInt = Int16.Parse(value.startDate.Substring(0, 4));
                var MonthInt = Int16.Parse(value.startDate.Substring(4, 2));
                var dayInt = Int16.Parse(value.startDate.Substring(6, 2));
                var dayOfMonth = DateTime.DaysInMonth(yearInt, MonthInt);

                List<EventCalendarGroup> items = new List<EventCalendarGroup>();
                for (int i = 0; i < dayOfMonth; i++)
                {
                    var day = new DateTime(yearInt, MonthInt, (i+1));

                    var dateDay = day.toBetweenDate();

                    var data = docsEventCalendar.Where(c => dateDay.start >= c.dateStart.toDateFromString().toBetweenDate().start && dateDay.end <= c.dateEnd.toDateFromString().toBetweenDate().end);
                    if (data.Count() > 0)
                    {
                        var item = new EventCalendarGroup { date = day.ToString("yyyyMMdd"), length = data.Count(), items = data.ToList() };
                        items.Add(item);
                    }
                }

                //items = items.Skip(value.skip).Take(value.limit).ToList();

                return new Response { status = "S", message = "success", objectData = items, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("mark/read")]
        public ActionResult<Response> MarkRead([FromBody] EventCalendar value)
        {
            try
            {
                if (value.year == 0) {
                    return new Response { status = "E", message = "Error input year." };
                }
                var col = new Database().MongoClient<EventCalendar>( "eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());
                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<EventCalendar>.Filter.Eq("language", value.language); }

                var docs = col.Find(filter).Project(c => new { c.dateStart, c.dateEnd }).ToList();
                    
                var json = "";
                int year = value.year;
                string mount = "";
                string day = "";
                string doc = "";
                int dayPerMount = 0;
                int yearStart = year - 1;
                int yearEnd = year + 1;
                int[] YearDaysPerMonth = new int[] { };

                for (int j = yearStart; j <= yearEnd; j++)
                {
                    year = j;
                    bool leap = IsLeapYear(year);
                    if (leap)
                        YearDaysPerMonth = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                    else
                        YearDaysPerMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

                    for (int k = 1; k <= 12; k++)
                    {
                        dayPerMount = YearDaysPerMonth[k - 1];
                        mount = k.ToString();
                        if (mount.Length < 2)
                            mount = "0" + k;

                        for (int l = 1; l <= dayPerMount; l++)
                        {
                            day = l.ToString();
                            if (day.Length < 2)
                                day = "0" + l;

                            bool dateBetween = false;
                            string date = year + mount + day;

                            foreach (var c in docs)
                            {
                                dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                                if (dateBetween)
                                {
                                    break;
                                }
                                //do something
                            }

                            if (dateBetween)
                                json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":true,\"selected\":false}";
                            else
                                json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":false,\"selected\":false}";

                            if (doc == "")
                                doc = "{" + json;
                            else
                                doc = doc + "," + json;

                        }
                    }
                }

                doc += "}";

                return new Response { status = "S", message = "Success", jsonData = doc };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("mark/read2")]
        public ActionResult<Response> MarkRead2([FromBody] EventCalendar value)
        {
            try
            {
                if (value.year == 0)
                {
                    return new Response { status = "E", message = "Error input year." };
                }
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());
                filter &= (Builders<EventCalendar>.Filter.Ne("dateStart", "Invalid date")
                    & Builders<EventCalendar>.Filter.Ne("dateEnd", "Invalid date")
                    & Builders<EventCalendar>.Filter.Ne("dateStart", "")
                    & Builders<EventCalendar>.Filter.Ne("dateEnd", ""));

                if (!string.IsNullOrEmpty(value.language)) { filter &= Builders<EventCalendar>.Filter.Eq("language", value.language); }

                var docs = col.Find(filter).Project(c => new {
                    c.dateStart,
                    c.dateEnd,
                    c.code,
                    c.title,
                    c.imageUrl,
                    c.createBy,
                    c.imageUrlCreateBy,
                    c.createDate,
                    c.description,
                    c.updateBy,
                    c.updateDate,
                    c.category,
                    c.view,
            }).ToList();

                var json = "";
                var model = new List<EventCalendarMark2>();
                int year = value.year;
                string mount = "";
                string day = "";
                string doc = "";
                int dayPerMount = 0;
                int yearStart = year - 1;
                int yearEnd = year + 1;
                int[] YearDaysPerMonth = new int[] { };

                //for (int j = yearStart; j <= yearEnd; j++)
                //{
                //year = j;
                bool leap = IsLeapYear(year);
                if (leap)
                    YearDaysPerMonth = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                else
                    YearDaysPerMonth = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

                for (int k = 1; k <= 12; k++)
                {
                    dayPerMount = YearDaysPerMonth[k - 1];
                    mount = k.ToString();
                    if (mount.Length < 2)
                        mount = "0" + k;

                    for (int l = 1; l <= dayPerMount; l++)
                    {
                        day = l.ToString();
                        if (day.Length < 2)
                            day = "0" + l;

                        bool dateBetween = false;
                        string curDate = year + mount + day;

                        model.Add(new EventCalendarMark2
                        {
                            date = curDate
                        });

                        foreach (var c in docs)
                        {
                            dateBetween = Between(curDate.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                            if (dateBetween)
                            {
                                model.ForEach(o =>
                                {
                                    if (o.date == curDate)
                                    {
                                        o.items.Add(new EventDataMark2
                                        {
                                            code = c.code,
                                            title = c.title,
                                            dateStart = c.dateStart,
                                            dateEnd = c.dateEnd,
                                            imageUrl = c.imageUrl,
                                            createBy = c.createBy,
                                            imageUrlCreateBy = c.imageUrlCreateBy,
                                            createDate = c.createDate,
                                            description = c.description,
                                            updateBy = c.updateBy,
                                            updateDate = c.updateDate,
                                            category = c.category,
                                            view = c.view,
                                        });
                                    }
                                });
                            }
                            //do something
                        }

                        //if (dateBetween)
                        //    json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":true,\"selected\":false}";
                        //else
                        //    json = "\"" + year + "-" + mount + "-" + day + "\":{\"marked\":false,\"selected\":false}";

                        //if (doc == "")
                        //    doc = "{" + json;
                        //else
                        //    doc = doc + "," + json;

                    }
                }
                //}

                //doc += "}";

                return new Response { status = "S", message = "Success", jsonData = doc, objectData = model };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>( "eventCalendarGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("galleryFile/read")]
        public ActionResult<Response> GalleryFileRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("eventCalendarGalleryFile");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).SortBy(o => o.title).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl, c.code, c.type, c.title, c.size }).ToList();

                docs = docs.OrderBy(c => c.title.PadNumbers()).ToList();
                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<Comment>("eventCalendarComment");

                var filter = Builders<Comment>.Filter.Ne("status", "D") & Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                List<Comment> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit).ToList();

                docs.ForEach(c =>
                {
                    if (!string.IsNullOrEmpty(c.profileCode))
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", c.profileCode);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                        c.imageUrlCreateBy = docRegister.imageUrl;
                    }
                });

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("comment/create")]
        public ActionResult<Response> CommentCreate([FromBody] Comment value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {

                //Get Profile
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode);
                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                var word = value.description.verifyRude();

                var col = new Database().MongoClient("eventCalendarComment");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "description", word },
                    { "original", value.description },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "profileCode", value.profileCode },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
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

        #region category

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>( "eventCalendarCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Category>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        #endregion

        public static bool IsLeapYear(int year)
        {
            if (year % 400 == 0)
            {
                return true;
            }
            if (year % 100 == 0)
            {
                return false;
            }
            //otherwise
            return (year % 4) == 0;
        }
        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input >= date1 && input <= date2);
        }

        // POST /read
        [HttpPost("count")]
        public ActionResult<Response> Count([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = (Builders<EventCalendar>.Filter.Eq("status", "A") & value.filterOrganization<EventCalendar>());
                var docs = col.CountDocuments(filter);

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
    }

}