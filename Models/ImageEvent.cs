using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class ImageEvent : Identity
    {

        public ImageEvent()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            view = 0;
            totalLv = 0;
            publishDate = "";
            publishDateTime = DateTime.Now;
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public int totalLv { get; set; }
        public string publishDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime publishDateTime { get; set; }
    }
}
