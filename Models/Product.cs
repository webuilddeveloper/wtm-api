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
            imageUrl = "";
            imageUrlCreateBy = "";
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
    }
}
