using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Product : Identity
    {

        public Product()
        {
            status2 = "";
            imageUrl = "";
            imageBanner = "";
            imageExample = "";
            imageUrlCreateBy = "";
            view = 0;
            progress = 0;
            functionList = "";
        }

        public string status2 { get; set; }
        public string imageUrl { get; set; }
        public string imageBanner { get; set; }
        public string imageExample { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public int progress { get; set; }
        public String functionList { get; set; }
    }

    public class ProductFunction
    {
        public ProductFunction()
        {
            title = "";
            description = "";
        }

        public string title { get; set; }
        public string description { get; set; }
    }
}
