using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class News : Identity
    {

        public News()
        {
            imageUrl = "";
            imageBanner = "";
            imageUrlCreateBy = "";
            productCode = "";
            view = 0;
            totalLv = 0;
        }

        public string imageUrl { get; set; }
        public string productCode { get; set; }
        public string imageBanner { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public int totalLv { get; set; }
    }
}
