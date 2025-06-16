using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Register : Identity
    {

        public Register()
        {
            token = "";
            imageUrl = "";
            username = "";
            password = "";
            reference = "";

            prefixName = "";
            prefixNameEN = "";
            firstName = "";
            lastName = "";
            firstNameEN = "";
            lastNameEN = "";
            birthDay = "";
            phone = "";
            email = "";
            appleID = "";
            facebookID = "";
            googleID = "";
            lineID = "";
            line = "";

            sex = "";
            soi = "";
            address = "";
            moo = "";
            road = "";
            tambon = "";
            amphoe = "";
            province = "";
            postno = "";
            job = "";
            idcard = "";
            officerCode = "";
            licenseNumber = "";
            countUnit = "";

            tambonCode = "";
            amphoeCode = "";
            provinceCode = "";
            postnoCode = "";

            linkAccount = "";
            codeShortNumber = "";
            dateOfIssue = "";
            dateOfExplry = "";
            reNewTo = "";
            vetCategory = "";
            vetImageUrl = "";
            examname = "";
            resultname = "";
            row = 1;
            agenda = 0;
            position = "";
            positionEN = "";
            ageRange = "";
            ref_code = "";
            categoryName = "";
        }

        public string token { get; set; }
        public bool isOnline { get; set; }
        public string imageUrl { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string reference { get; set; }

        public string prefixName { get; set; }
        public string prefixNameEN { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string firstNameEN { get; set; }
        public string lastNameEN { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string appleID { get; set; }
        public string facebookID { get; set; }
        public string googleID { get; set; }
        public string lineID { get; set; }
        public string line { get; set; }

        public string newPassword { get; set; }

        public string sex { get; set; }
        public string soi { get; set; }
        public string address { get; set; }
        public string moo { get; set; }
        public string road { get; set; }
        public string tambon { get; set; }
        public string amphoe { get; set; }
        public string province { get; set; }
        public string postno { get; set; }
        public string job { get; set; }
        public string idcard { get; set; }
        public string officerCode { get; set; }
        public string licenseNumber { get; set; }
        public string countUnit { get; set; }
        public string tambonCode { get; set; }
        public string amphoeCode { get; set; }
        public string provinceCode { get; set; }
        public string postnoCode { get; set; }

        public string linkAccount { get; set; }

        public string codeShortNumber { get; set; }
        public string dateOfIssue { get; set; }
        public string dateOfExplry { get; set; }
        public string reNewTo { get; set; }
        public string vetCategory { get; set; }
        public string vetImageUrl { get; set; }
        public string examname { get; set; }
        public string resultname { get; set; }
        public int row { get; set; }
        public int agenda { get; set; }
        public string position { get; set; }
        public string positionEN { get; set; }
        public string ageRange { get; set; }
        public string ref_code { get; set; }
        public string categoryName { get; set; }
    }

    public class registerRole : Identity
    {
        public string username { get; set; }
    }
}
