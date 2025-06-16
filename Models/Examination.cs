using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Examination : Identity
    {
        public Examination()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            view = 0;
            status2 = false;
            totalAnswer = 0;
            totalPoints = 0;
            points = 0;
            colorpass = 0;
            pass = "";
            knowledge = "";
            reference = "";
        }

        public int view { get; set; }
        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public bool status2 { get; set; }
        public int totalAnswer { get; set; }
        public int totalPoints { get; set; }
        public int points { get; set; }
        public string pass { get; set; }
        public UInt64 colorpass { get; set; }
        public List<Questions> questions { get; set; }
        public List<Knowledge> knowledgeList { get; set; }
        public string knowledge { get; set; }
        public string reference { get; set; }
    }
}