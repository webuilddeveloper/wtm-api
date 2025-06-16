using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Employee : Identity
    {

        public Employee()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            position = "";
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string position { get; set; }
    }
}
