using MongoDB.Bson.Serialization.Attributes;
using System;
namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class ExaminationAnswer : Identity
    {
        public ExaminationAnswer()
        {
            reference = "";
            points = 0;
        }

        public string reference { get; set; }
        public bool value { get; set; }
        public int points { get; set; }

    }
}
