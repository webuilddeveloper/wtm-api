using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Insurance : Identity
    {

        public Insurance()
        {
            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            idcard = "";
            phone = "";
            email = "";
            key = "";
            page = "";
            reference = "";
            isAccept = false;
            status2 = "";
        }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string idcard { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public bool isAccept { get; set; }

        public string key { get; set; }
        public string page { get; set; }
        public string reference { get; set; }
        public string status2 { get; set; }
    }
}
