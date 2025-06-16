using System;
using System.Net.Http;
using System.Net.Http.Headers;
using cms_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace cms_api.Extension
{
    public class StatisticService
    {
        public StatisticService(string contentCode, string content, string type, string profileCode, string firstName, string lastName)
        {
            this.Create(contentCode, content, type, profileCode, firstName, lastName);
        }

        private void Create(string contentCode, string content, string type, string profileCode, string firstName, string lastName) {
            try
            {
                var col = new Database().MongoClient("register");
                var filter = Builders<BsonDocument>.Filter.Eq("code", profileCode);
                var doc = col.Find(filter).FirstOrDefault();

                var obj = new { application = new Config().application, contentCode = contentCode, content = content, type = type, profileCode = profileCode, firstName = doc["firstName"].ToString(), lastName = doc["lastName"].ToString() };

                HttpClient client = new HttpClient();
                var json = JsonConvert.SerializeObject(obj);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.PostAsync("http://core148.we-builds.com/st-api/api/Statistic/Create", httpContent);

            }
            catch { }
        }
    }
}
