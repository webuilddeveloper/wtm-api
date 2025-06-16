using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Response
    {
        public Response()
        {
            status2 = false;
        }

        public string status { get; set; }
        public string message { get; set; }
        public string jsonData { get; set; }
        public object objectData { get; set; }
        public object nearByData { get; set; }
        public long totalData { get; set; }
        public bool status2 { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Response2
    {
        public Response2()
        {
            status2 = false;
        }

        public string status { get; set; }
        public string message { get; set; }
        public string jsonData { get; set; }
        public List<obj> objectData { get; set; }
        public object nearByData { get; set; }
        public long totalData { get; set; }
        public bool status2 { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class obj
    {
        public obj()
        {
            code = "";
            title = "";
            titleEN = "";
            imageUrl = "";
            category = "";
            description = "";
            descriptionEN = "";
            createBy = "";
            createDate = "";
            createTime = "";
            updateBy = "";
            updateDate = "";
            updateTime = "";
            status = "";
            docTime = "";
            center = "";
            textButton = "";
            linkUrl = "";
            fileUrl = "";
            imageUrlCreateBy = "";
            dateStart = "";
            dateEnd = "";
            linkYoutube = "";
            linkFacebook = "";
            confirmStatus = "";

            author = "";
            publisher = "";
            bookType = "";
            numberOfPages = "";
            size = "";
        }
        public string code { get; set; }
        public string title { get; set; }
        public string titleEN { get; set; }
        public string imageUrl { get; set; }
        public int sequence { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public string descriptionEN { get; set; }
        public string createBy { get; set; }
        public string createDate { get; set; }
        public string createTime { get; set; }
        public string updateBy { get; set; }
        public string updateDate { get; set; }
        public string updateTime { get; set; }
        public bool isActive { get; set; }
        public string status { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }
        public bool isNotification { get; set; }
        public DateTime docDate { get; set; }
        public string docTime { get; set; }
        public string center { get; set; }
        public string textButton { get; set; }
        public string linkUrl { get; set; }
        public string fileUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string linkYoutube { get; set; }
        public string linkFacebook { get; set; }
        public string confirmStatus { get; set; }

        public string author { get; set; }
        public string publisher { get; set; }
        public string bookType { get; set; }
        public string numberOfPages { get; set; }
        public string size { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class obj2
    {
        public obj2()
        {
            code = "";
            title = "";
            titleEN = "";
            imageUrl = "";
            category = "";
            description = "";
            descriptionEN = "";
            createBy = "";
            createDate = "";
            createTime = "";
            updateBy = "";
            updateDate = "";
            updateTime = "";
            status = "";
            docTime = "";
            center = "";
            textButton = "";
            linkUrl = "";
            fileUrl = "";
            imageUrlCreateBy = "";
            dateStart = "";
            dateEnd = "";
            linkYoutube = "";
            linkFacebook = "";
            confirmStatus = "";

            author = "";
            publisher = "";
            bookType = "";
            numberOfPages = "";
            size = "";
        }
        public string code { get; set; }
        public string title { get; set; }
        public string titleEN { get; set; }
        public string imageUrl { get; set; }
        public int sequence { get; set; }
        public string category { get; set; }
        public List<Category> categoryList { get; set; }
        public string description { get; set; }
        public string descriptionEN { get; set; }
        public string createBy { get; set; }
        public string createDate { get; set; }
        public string createTime { get; set; }
        public string updateBy { get; set; }
        public string updateDate { get; set; }
        public string updateTime { get; set; }
        public bool isActive { get; set; }
        public string status { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }
        public bool isNotification { get; set; }
        public DateTime docDate { get; set; }
        public string docTime { get; set; }
        public string center { get; set; }
        public string textButton { get; set; }
        public string linkUrl { get; set; }
        public string fileUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string linkYoutube { get; set; }
        public string linkFacebook { get; set; }
        public string confirmStatus { get; set; }

        public string author { get; set; }
        public string publisher { get; set; }
        public string bookType { get; set; }
        public string numberOfPages { get; set; }
        public string size { get; set; }
    }
}
