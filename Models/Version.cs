using System;
using cms_api.Extension;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Version
    {
        public Version()
        {
            code = code.toCode();
            createDate = DateTime.Now.toStringFromDate();
            createTime = DateTime.Now.toTimeStringFromDate();
            updateDate = DateTime.Now.toStringFromDate();
            updateTime = DateTime.Now.toTimeStringFromDate();
            docDate = DateTime.Now.Date.AddHours(7);
            docTime = DateTime.Now.toTimeStringFromDate();
            isActive = true;
            isDelete = false;
        }

        public string platform { get; set; }
        public string version { get; set; }
        public bool isForce { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string note { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string code { get; set; }
        public string createBy { get; set; }
        public string createDate { get; set; }
        public string createTime { get; set; }
        public string updateBy { get; set; }
        public string updateDate { get; set; }
        public string updateTime { get; set; }
        public DateTime docDate { get; set; }
        public string docTime { get; set; }
        public bool isActive { get; set; }
        public bool isDelete { get; set; }
    }
}
