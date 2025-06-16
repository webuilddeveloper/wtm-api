using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using cms_api.Models;
using Jose;
using master_api.Controllers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace cms_api.Extension
{
    public static class Extension
    {
        public static string toCode(this string param)
        {
            return $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-{DateTime.Now.Millisecond.ToString()}-{new Random().Next(100, 999)}";
        }
        public static string getRandom(this string param)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[8];
            Random random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return $"{new string(stringChars)}";
        }

        public static DateTime toDateFromString(this string param)
        {
            if (param.Length == 8) //20200131
            {
                return new DateTime(Int16.Parse(param.Substring(0, 4)), Int16.Parse(param.Substring(4, 2)), Int16.Parse(param.Substring(6, 2)));
            }
            else if (param.Length == 14) //20200131120000
            {
                return new DateTime(Int16.Parse(param.Substring(0, 4)), Int16.Parse(param.Substring(4, 2)), Int16.Parse(param.Substring(6, 2)), Int16.Parse(param.Substring(8, 2)), Int16.Parse(param.Substring(10, 2)), Int16.Parse(param.Substring(12, 2)));
            }

            return DateTime.Now;
        }

        public static BetweenDate toBetweenDate(this DateTime param)
        {
            return new BetweenDate { start = new DateTime(param.Year, param.Month, param.Day, 0, 0, 0), end = new DateTime(param.Year, param.Month, param.Day, 23, 59, 59) };
        }

        public static string toStringFromDate(this DateTime param)
        {
            var d = param.ToString("yyyyMMddHHmmss");
            return d;
        }

        public static string toTimeStringFromDate(this DateTime param)
        {
            return new TimeSpan(param.Hour, param.Minute, param.Second).ToString();
        }

        public static string toEncode(this string param)
        {
            var secretKey = new byte[] { 164, 60, 194, 0, 161, 189, 41, 38, 130, 89, 141, 164, 45, 170, 159, 209, 69, 137, 243, 216, 191, 131, 47, 250, 32, 107, 231, 117, 37, 158, 225, 234 };
            return JWT.Encode(param, secretKey, JwsAlgorithm.HS256);
        }

        public static string toDecode(this string param)
        {
            var secretKey = new byte[] { 164, 60, 194, 0, 161, 189, 41, 38, 130, 89, 141, 164, 45, 170, 159, 209, 69, 137, 243, 216, 191, 131, 47, 250, 32, 107, 231, 117, 37, 158, 225, 234 };
            return JWT.Decode(param, secretKey, JwsAlgorithm.HS256);
        }

        public static void logCreate(this object param, string title, string description)
        {
            var doc = new BsonDocument();
            var col = new Database().MongoClient("log_" + title);

            try
            {
                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", title },
                    { "description", description },
                    { "raw", param.ToJson() },
                    { "createBy", param.GetType().GetProperty("createBy").GetValue(param, null).ToString() },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", param.GetType().GetProperty("updateBy").GetValue(param, null).ToString() },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                col.InsertOne(doc);
            }
            catch (Exception ex)
            {
                doc = new BsonDocument
                {
                    { "code", "".toCode() },
                    { "title", title },
                    { "description", description },
                    { "raw", ex.Message },
                    { "createBy", param.GetType().GetProperty("createBy").GetValue(param, null).ToString() },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", param.GetType().GetProperty("updateBy").GetValue(param, null).ToString() },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true }
                };
                col.InsertOne(doc);
            }
        }

        public static async Task statisticsCreateAsync(this Criteria value, string page)
        {
            try
            {
                value.databaseName = "vet_prod_statistics";
                value.page = page;

                HttpClient client = new HttpClient();
                var json = JsonConvert.SerializeObject(value);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync("http://vet.we-builds.com/statistic-api/statistics/create", httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

            }
            catch
            {

            }
        }

        public static FilterDefinition<T> filterPermission<T>(this string param, string field)
        {
            var permissionFilter = Builders<T>.Filter.Ne("status", "D");

            var permission = param.Split(",");
            for (int i = 0; i < permission.Length; i++)
            {
                if (i == 0)
                    permissionFilter = Builders<T>.Filter.Eq(field, permission[i]);
                else
                    permissionFilter |= Builders<T>.Filter.Eq(field, permission[i]);
            }

            return permissionFilter;
        }

        public static FilterDefinition<T> filterOrganization<T>(this string param, string field)
        {
            var organizationFilter = Builders<T>.Filter.Ne("status", "D");

            var organization = param.Split(",");
            for (int i = 0; i < organization.Length; i++)
            {
                if (i == 0)
                    organizationFilter = Builders<T>.Filter.Regex(field, organization[i]);
                else
                    organizationFilter |= Builders<T>.Filter.Regex(field, organization[i]);
            }

            return organizationFilter;
        }

        public static FilterDefinition<T> filterOrganization<T>(this Criteria param)
        {
            if (!string.IsNullOrEmpty(param.profileCode))
            {
                param.organization = new List<Organization>();

                //get Organization
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq("code", param.profileCode);
                var doc = col.Find(filter).Project(c => c.countUnit).FirstOrDefault();

                if (!string.IsNullOrEmpty(doc))
                {
                    var model = JsonConvert.DeserializeObject<List<CountUnit>>(doc);

                    model.ForEach(c =>
                    {
                        param.organization.Add(new Organization
                        {
                            lv0 = c.lv0,
                            lv1 = c.lv1,
                            lv2 = c.lv2,
                            lv3 = c.lv3,
                            lv4 = c.lv4,
                            status = c.status
                        });
                    });
                }
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D");

            // permission != "all" check for web-shared
            if (param.permission != "all")
            {
                if (param.organization.Count == 0)
                {
                    return publicFilter &= Builders<T>.Filter.Eq("isPublic", true);
                }
                else
                {
                    publicFilter &= Builders<T>.Filter.Eq("isPublic", true);
                    //publicFilter = (Builders<T>.Filter.Eq("lv0", "x") & Builders<T>.Filter.Eq("lv1", "x") & Builders<T>.Filter.Eq("lv2", "x") & Builders<T>.Filter.Eq("lv3", "x") & Builders<T>.Filter.Eq("lv4", "x"));

                    param.organization.ForEach(c =>
                    {
                        if (c.status == "A")
                        {
                            // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                            // Use 'Regex' because where lv like in content

                            var organizationFilter = Builders<T>.Filter.Ne("status", "D");

                            if (!string.IsNullOrEmpty(c.lv4))
                            {
                                var organization = c.lv4.Split(",");
                                for (int i = 0; i < organization.Length; i++)
                                {
                                    if (i == 0)
                                        organizationFilter = Builders<T>.Filter.Regex("lv4", organization[i]);
                                    else
                                        organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                                }

                                publicFilter |= (organizationFilter);
                            }
                            else if (!string.IsNullOrEmpty(c.lv3))
                            {
                                var organization = c.lv3.Split(",");
                                for (int i = 0; i < organization.Length; i++)
                                {
                                    if (i == 0)
                                        organizationFilter = Builders<T>.Filter.Regex("lv3", organization[i]);
                                    else
                                        organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                                }

                                publicFilter |= (organizationFilter);
                            }
                            else if (!string.IsNullOrEmpty(c.lv2))
                            {
                                var organization = c.lv2.Split(",");
                                for (int i = 0; i < organization.Length; i++)
                                {
                                    if (i == 0)
                                        organizationFilter = Builders<T>.Filter.Regex("lv2", organization[i]);
                                    else
                                        organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                                }

                                publicFilter |= (organizationFilter);
                            }
                            else if (!string.IsNullOrEmpty(c.lv1))
                            {
                                var organization = c.lv1.Split(",");
                                for (int i = 0; i < organization.Length; i++)
                                {
                                    if (i == 0)
                                        organizationFilter = Builders<T>.Filter.Regex("lv1", organization[i]);
                                    else
                                        organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                                }

                                publicFilter |= (organizationFilter);
                            }
                            else if (!string.IsNullOrEmpty(c.lv0))
                            {
                                var organization = c.lv0.Split(",");
                                for (int i = 0; i < organization.Length; i++)
                                {
                                    if (i == 0)
                                        organizationFilter = Builders<T>.Filter.Regex("lv0", organization[i]);
                                    else
                                        organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                                }

                                publicFilter |= (organizationFilter);
                            }
                        }
                    });
                }
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static FilterDefinition<T> filterOrganization<T>(this EventCalendar param)
        {
            if (!string.IsNullOrEmpty(param.profileCode))
            {
                param.organization = new List<Organization>();

                //get Organization
                var col = new Database().MongoClient<Register>("register");
                var filter = Builders<Register>.Filter.Eq("code", param.profileCode);
                var doc = col.Find(filter).Project(c => c.countUnit).FirstOrDefault();
                var model = JsonConvert.DeserializeObject<List<CountUnit>>(doc);
                model.ForEach(c =>
                {
                    param.organization.Add(new Organization
                    {
                        lv0 = c.lv0,
                        lv1 = c.lv1,
                        lv2 = c.lv2,
                        lv3 = c.lv3,
                        lv4 = c.lv4,
                        status = c.status
                    });
                });
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D");

            if (param.organization.Count == 0)
            {
                return publicFilter &= Builders<T>.Filter.Eq("isPublic", true);
            }
            else
            {
                publicFilter &= Builders<T>.Filter.Eq("isPublic", true);
                //publicFilter = (Builders<T>.Filter.Eq("lv0", "") & Builders<T>.Filter.Eq("lv1", "") & Builders<T>.Filter.Eq("lv2", "") & Builders<T>.Filter.Eq("lv3", "") & Builders<T>.Filter.Eq("lv4", ""));

                param.organization.ForEach(c =>
                {
                    if (c.status == "A")
                    {
                        // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                        // Use 'Regex' because where lv like in content

                        var organizationFilter = Builders<T>.Filter.Ne("status", "D");

                        if (!string.IsNullOrEmpty(c.lv4))
                        {
                            var organization = c.lv4.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv4", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv3))
                        {
                            var organization = c.lv3.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv3", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv2))
                        {
                            var organization = c.lv2.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv2", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv1))
                        {
                            var organization = c.lv1.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv1", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                        else if (!string.IsNullOrEmpty(c.lv0))
                        {
                            var organization = c.lv0.Split(",");
                            for (int i = 0; i < organization.Length; i++)
                            {
                                if (i == 0)
                                    organizationFilter = Builders<T>.Filter.Regex("lv0", organization[i]);
                                else
                                    organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                            }

                            publicFilter |= (organizationFilter);
                        }
                    }
                });
            }

            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static FilterDefinition<T> filterOrganizationOld<T>(this Criteria param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )


            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = (Builders<T>.Filter.Eq("lv0", "")
                & Builders<T>.Filter.Eq("lv1", "")
                & Builders<T>.Filter.Eq("lv2", "")
                & Builders<T>.Filter.Eq("lv3", "")
                & Builders<T>.Filter.Eq("lv4", "")
                & Builders<T>.Filter.Eq("lv5", ""));




            // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
            // Use 'Regex' because where lv like in content
            var lv5Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv5))
            {
                var organization = param.lv5.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv5Filter = Builders<T>.Filter.Regex("lv5", organization[i]);
                    else
                        lv5Filter |= Builders<T>.Filter.Regex("lv5", organization[i]);
                }

                return (lv5Filter);
            }
            var lv4Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv4))
            {
                var organization = param.lv4.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv4Filter = Builders<T>.Filter.Regex("lv4", organization[i]);
                    else
                        lv4Filter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                }

                return (lv4Filter);
            }
            var lv3Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv3))
            {
                var organization = param.lv3.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv3Filter = Builders<T>.Filter.Regex("lv3", organization[i]);
                    else
                        lv3Filter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                }

                return (lv3Filter);
            }
            var lv2Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv2))
            {
                var organization = param.lv2.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv2Filter = Builders<T>.Filter.Regex("lv2", organization[i]);
                    else
                        lv2Filter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                }

                return (lv2Filter);
            }
            var lv1Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv1))
            {
                var organization = param.lv1.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv1Filter = Builders<T>.Filter.Regex("lv1", organization[i]);
                    else
                        lv1Filter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                }

                return (lv1Filter);
            }
            var lv0Filter = Builders<T>.Filter.Ne("status", "D");
            if (!string.IsNullOrEmpty(param.lv0))
            {
                var organization = param.lv0.Split(",");
                for (int i = 0; i < organization.Length; i++)
                {
                    if (i == 0)
                        lv0Filter = Builders<T>.Filter.Regex("lv0", organization[i]);
                    else
                        lv0Filter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                }

                return (lv0Filter);
            }


            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static FilterDefinition<T> filterOrganizationOld<T>(this EventCalendar param)
        {
            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )

            // (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '')
            var publicFilter = Builders<T>.Filter.Ne("status", "D"); // <----- ผิดตรงนี้

            if (param.organization.Count == 0)
                return publicFilter = Builders<T>.Filter.Ne("status", "D");

            param.organization.ForEach(c =>
            {
                if (c.status == "A")
                {
                    // (lv0 = 'xxx' & lv1 = 'xxx' & lv2 = 'xxx' & lv3 = 'xxx')
                    // Use 'Regex' because where lv like in content

                    var organizationFilter = Builders<T>.Filter.Ne("status", "D");

                    if (!string.IsNullOrEmpty(c.lv4))
                    {
                        var organization = c.lv4.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv4", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv4", organization[i]);
                        }

                        publicFilter |= (organizationFilter); // <----- ผิดตรงนี้
                    }
                    else if (!string.IsNullOrEmpty(c.lv3))
                    {
                        var organization = c.lv3.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv3", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv3", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv2))
                    {
                        var organization = c.lv2.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv2", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv2", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv1))
                    {
                        var organization = c.lv1.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv1", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv1", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                    else if (!string.IsNullOrEmpty(c.lv0))
                    {
                        var organization = c.lv0.Split(",");
                        for (int i = 0; i < organization.Length; i++)
                        {
                            if (i == 0)
                                organizationFilter = Builders<T>.Filter.Regex("lv0", organization[i]);
                            else
                                organizationFilter |= Builders<T>.Filter.Regex("lv0", organization[i]);
                        }

                        publicFilter |= (organizationFilter);
                    }
                }
            });


            // (status != 'D' & ( (lv0 = '' & lv1 = '' & lv2 = '' & lv3 = '') | ( (lv0 = 'xxx') & (lv1 = 'xxx') & (lv2 = 'xxx') & (lv3 = 'xxx') ) ) )
            //return ( publicFilter | ( (lv0Filter) & (lv1Filter) & (lv2Filter) & (lv3Filter) & (lv4Filter) & (lv5Filter) ) );
            return (publicFilter);
        }

        public static Criteria filterQrganizationAuto(this List<Organization> param)
        {
            var value = new Criteria();

            foreach (var item in param)
            {
                if (!string.IsNullOrEmpty(item.lv4))
                {
                    value.lv0 = "";
                    value.lv1 = "";
                    value.lv2 = "";
                    value.lv3 = "";
                    var split = item.lv4.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {
                        var col2 = new Database().MongoClient<News>("organization");
                        var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv4");
                        var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                        if (doc2 != null)
                        {
                            if (string.IsNullOrEmpty(value.lv4))
                                value.lv4 = doc2.code;
                            else
                                value.lv4 = value.lv4 + "," + doc2.code;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv3))
                {
                    value.lv0 = "";
                    value.lv1 = "";
                    value.lv2 = "";
                    var split = item.lv3.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {

                        //lv3
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv3");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                            if (doc2 != null)
                            {
                                if (string.IsNullOrEmpty(value.lv3))
                                    value.lv3 = doc2.code;
                                else
                                    value.lv3 = value.lv3 + "," + doc2.code;
                            }
                        }

                        //lv4
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", split[i]) & Builders<News>.Filter.Eq("category", "lv4");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                            if (doc2.Count > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv4))
                                    value.lv4 = string.Join(",", doc2.Select(c => c.code).ToList());
                                else
                                    value.lv4 = value.lv4 + "," + string.Join(",", doc2.Select(c => c.code).ToList());
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv2))
                {
                    value.lv0 = "";
                    value.lv1 = "";

                    var split = item.lv2.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {

                        //lv2
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv2");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                            if (doc2 != null)
                            {
                                if (string.IsNullOrEmpty(value.lv2))
                                    value.lv2 = doc2.code;
                                else
                                    value.lv2 = value.lv2 + "," + doc2.code;
                            }
                        }

                        //lv3
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", split[i]) & Builders<News>.Filter.Eq("category", "lv3");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                            if (doc2.Count > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv3))
                                    value.lv3 = string.Join(",", doc2.Select(c => c.code).ToList());
                                else
                                    value.lv3 = value.lv3 + "," + string.Join(",", doc2.Select(c => c.code).ToList());
                            }

                            doc2.ForEach(c =>
                            {
                                //lv4
                                {
                                    var col3 = new Database().MongoClient<News>("organization");
                                    var filter3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", c.code) & Builders<News>.Filter.Eq("category", "lv4");
                                    var doc3 = col3.Find(filter3).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                    if (doc3.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv4))
                                            value.lv4 = string.Join(",", doc3.Select(c => c.code).ToList());
                                        else
                                            value.lv4 = value.lv4 + "," + string.Join(",", doc3.Select(c => c.code).ToList());
                                    }
                                }
                            });
                        }

                    }
                }
                else if (!string.IsNullOrEmpty(item.lv1))
                {
                    value.lv0 = "";
                    var split = item.lv1.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {

                        //lv1
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv1");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                            if (doc2 != null)
                            {
                                if (string.IsNullOrEmpty(value.lv1))
                                    value.lv1 = doc2.code;
                                else
                                    value.lv1 = value.lv1 + "," + doc2.code;
                            }
                        }

                        //lv2
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", split[i]) & Builders<News>.Filter.Eq("category", "lv2");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                            if (doc2.Count > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv2))
                                    value.lv2 = string.Join(",", doc2.Select(c => c.code).ToList());
                                else
                                    value.lv2 = value.lv2 + "," + string.Join(",", doc2.Select(c => c.code).ToList());
                            }

                            doc2.ForEach(c =>
                            {
                                //lv3
                                {
                                    var col3 = new Database().MongoClient<News>("organization");
                                    var filter3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", c.code) & Builders<News>.Filter.Eq("category", "lv3");
                                    var doc3 = col3.Find(filter3).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                    if (doc3.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv3))
                                            value.lv3 = string.Join(",", doc3.Select(c => c.code).ToList());
                                        else
                                            value.lv3 = value.lv3 + "," + string.Join(",", doc3.Select(c => c.code).ToList());
                                    }

                                    doc3.ForEach(cc =>
                                    {
                                        //lv4
                                        {
                                            var col4 = new Database().MongoClient<News>("organization");
                                            var filter4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", cc.code) & Builders<News>.Filter.Eq("category", "lv4");
                                            var doc4 = col4.Find(filter4).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                            if (doc4.Count > 0)
                                            {
                                                if (string.IsNullOrEmpty(value.lv4))
                                                    value.lv4 = string.Join(",", doc4.Select(c => c.code).ToList());
                                                else
                                                    value.lv4 = value.lv4 + "," + string.Join(",", doc4.Select(c => c.code).ToList());
                                            }
                                        }

                                    });
                                }
                            });
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(item.lv0))
                {
                    var split = item.lv0.Split(",");
                    for (int i = 0; i < split.Count(); i++)
                    {

                        //lv0
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("code", split[i]) & Builders<News>.Filter.Eq("category", "lv0");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).FirstOrDefault();
                            if (doc2 != null)
                            {
                                if (string.IsNullOrEmpty(value.lv0))
                                    value.lv0 = doc2.code;
                                else
                                    value.lv0 = value.lv0 + "," + doc2.code;
                            }
                        }

                        //lv1
                        {
                            var col2 = new Database().MongoClient<News>("organization");
                            var filter2 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv0", split[i]) & Builders<News>.Filter.Eq("category", "lv1");
                            var doc2 = col2.Find(filter2).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                            if (doc2.Count > 0)
                            {
                                if (string.IsNullOrEmpty(value.lv1))
                                    value.lv1 = string.Join(",", doc2.Select(c => c.code).ToList());
                                else
                                    value.lv1 = value.lv1 + "," + string.Join(",", doc2.Select(c => c.code).ToList());
                            }

                            doc2.ForEach(c =>
                            {
                                //lv2
                                {
                                    var col3 = new Database().MongoClient<News>("organization");
                                    var filter3 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv1", c.code) & Builders<News>.Filter.Eq("category", "lv2");
                                    var doc3 = col3.Find(filter3).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                    if (doc3.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(value.lv2))
                                            value.lv2 = string.Join(",", doc3.Select(c => c.code).ToList());
                                        else
                                            value.lv2 = value.lv2 + "," + string.Join(",", doc3.Select(c => c.code).ToList());
                                    }

                                    doc3.ForEach(cc =>
                                    {
                                        //lv3
                                        {
                                            var col4 = new Database().MongoClient<News>("organization");
                                            var filter4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv2", cc.code) & Builders<News>.Filter.Eq("category", "lv3");
                                            var doc4 = col4.Find(filter4).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                            if (doc4.Count > 0)
                                            {
                                                if (string.IsNullOrEmpty(value.lv3))
                                                    value.lv3 = string.Join(",", doc4.Select(c => c.code).ToList());
                                                else
                                                    value.lv3 = value.lv3 + "," + string.Join(",", doc4.Select(c => c.code).ToList());
                                            }

                                            doc4.ForEach(ccc =>
                                            {
                                                //lv4
                                                {
                                                    var col4 = new Database().MongoClient<News>("organization");
                                                    var filter4 = Builders<News>.Filter.Ne("status", "D") & Builders<News>.Filter.Eq("lv3", ccc.code) & Builders<News>.Filter.Eq("category", "lv4");
                                                    var doc4 = col4.Find(filter4).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();
                                                    if (doc4.Count > 0)
                                                    {
                                                        if (string.IsNullOrEmpty(value.lv4))
                                                            value.lv4 = string.Join(",", doc4.Select(c => c.code).ToList());
                                                        else
                                                            value.lv4 = value.lv4 + "," + string.Join(",", doc4.Select(c => c.code).ToList());
                                                    }
                                                }
                                            });
                                        }
                                    });
                                }
                            });
                        }
                    }
                }
            }

            return value;
        }

        public static List<BsonValue> ConvertToCaseInsensitiveRegexList(this IEnumerable<string> source)
        {
            return source.Select(range => new BsonRegularExpression("/^" + range.Replace("+", @"\+") + "$/i")).Cast<BsonValue>().ToList();
        }

        public static List<BsonValue> ConvertToEndsWithRegexList(this IEnumerable<string> source)
        {
            return source.Select(range => new BsonRegularExpression("/" + range.Replace("+", @"\+") + "$/i")).Cast<BsonValue>().ToList();
        }

        public static BsonRegularExpression ToCaseInsensitiveRegex(this string source)
        {
            return new BsonRegularExpression("/^" + source.Replace("+", @"\+") + "$/i");
        }

        public static string verifyRude(this string param)
        {
            var col = new Database().MongoClient<Rude>("mrude");
            var filter = Builders<Rude>.Filter.Eq("isActive", true);
            var docs = col.Find(filter).Project(c => new Rude { code = c.code, title = c.title }).ToList();

            docs.ForEach(c =>
            {
                param = System.Text.RegularExpressions.Regex.Unescape(param.Replace(c.title, "(***)").ToString());
            });

            return param;
        }

        public static string HtmlToPlainText(this string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        public static string ConvertStrToHtml(this string str)
        {
            var msg = HttpUtility.HtmlDecode(str);
            return msg;
        }

        public static void WriteLog(this Criteria model, string type, string page)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = "Logs\\";
            logFilePath = logFilePath + $"Log-{type}-{page}.txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            var strLog = $"{page} {model.code} {model.title} {model.updateBy} {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
            log.WriteLine(strLog);
            log.Close();
        }

        public static void statisticsCreate(this Criteria value, string page)
        {
            try
            {
                value.databaseName = "vet_prod_statistics";
                value.page = page;

                HttpClient client = new HttpClient();
                var json = JsonConvert.SerializeObject(value);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = client.PostAsync("http://opec.we-builds.com/statistic-api/statistics/create", httpContent);
                //var response = client.PostAsync("http://localhost:6100/statistics/create", httpContent);
                //var response = client.PostAsync("http://122.155.223.63/td-statistics-api/statistics/create", httpContent);

            }
            catch
            {

            }
        }

        public static string EncryptedData(this string data)
        {
            //string key = @"-----BEGIN PUBLIC KEY-----
            //MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAiMAjjCh7DcuzdrdZ
            //PwrxoZJ/kPONh9n95uTGlbJGYTTwSRZKAbcSL19mjDa1QfeEWZ2Pl9uaWjK8
            //iRKHNow/CsN68Kgc/HaOwT0VpDhnIFWB+mH7wy3I0xUDQtUBmmME3ZeDPKxk
            //Fd+TYruNEQfNPDq7b4wdy9mckFtJoMGZJ4xwSjqutvGytm/fgkTglUnDtHIQ
            //97Hx+UUhALyzIyhd+Unh8j8dtZelHu/vqJBZec8LFb53Hrkyb7NhZQbsV+pQ
            //y1Pdr6ywMuZbjMHhZNM8zjoawHr6pLtZqOrYvtemGfAwQV4bpMTg/oQAa0kX
            //EwdqSWtbedlKn1nHpr8lJra26N/D+d/UVMjGC9ejt7YoTrf/fbr9ClujsGtU
            //GhqUq/rVcdhzbAFpfQTQ3PXXe9SPMtXwFa04C/kpm2+k0Vwuv3itNUbmzI9c
            //MkoaWxIPCM0Y/3Hm0aQ0xSRqPJ9mrsp7HEd2j6J15NguGQLVClzjP68wRIyU
            //X2yOr4+7L5x5zqOWb93saUs27+Y22W7CK6V4JUzlEAhrIElHgWmUrt10r+Ct
            //IchZZeBC/3twcPmAt08/NZbDbqPDAoHTK/iBJE0Eu0MNqXbRvfMl+c2TrFER
            //sBZTTrmpCjWH2fUGbOR6t/7plh5mipb/nm1xpbdC2mzDadT2vUloo8/tuyJi
            //7fIa+OsCAwEAAQ==
            //-----END PUBLIC KEY-----";
            //string key = File.ReadAllText(@"C:\encrypt\public.txt");

            String pubKeyfile = @"C:\encrypt\public.txt";

            CryptoHelper cryptoHelper = new CryptoHelper();

            // Encrypt
            cryptoHelper.ImportPublicKey(pubKeyfile);
            string es = cryptoHelper.Encrypt(data);
            return es;
        }

        public static string PadNumbers(this string input)
        {
            return Regex.Replace(input.Trim(), "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }
    }
}

public class CryptoHelper
{
    RSACryptoServiceProvider rsaCryptoServiceProvider;
    RSA rsa;

    public CryptoHelper()
    {
        //rsaCryptoServiceProvider = new RSACryptoServiceProvider();
        rsa = RSA.Create();
    }

    public void ImportPublicKey(string fileName)
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa.importrsaprivatekey?view=netcore-3.1
        // https://vcsjones.dev/2019/10/07/key-formats-dotnet-3/
        // Read the file
        string text = File.ReadAllText(fileName);
        // Remove the "-----BEGIN PUBLIC KEY-----" etc.
        text = text.Replace("-----BEGIN PUBLIC KEY-----", "");
        text = text.Replace("-----END PUBLIC KEY-----", "");
        text = text.Replace("\r", "");
        text = text.Replace("\n", "");
        // Decode from Base64
        var privateKeyBytes = Convert.FromBase64String(text);
        // Now import
        int bytesRead;
        rsa.ImportSubjectPublicKeyInfo(privateKeyBytes, out bytesRead);
    }

    public string Encrypt(String s)
    {
        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(s);
        var encryptedData = rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA1);
        return Convert.ToBase64String(encryptedData);
    }
}