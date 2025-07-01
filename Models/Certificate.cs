using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Certificate : Identity
    {

        public Certificate()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            certified = "";
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string certified { get; set; }
    }
}
