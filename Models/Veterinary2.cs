using System;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Veterinary2 : Identity
    {
        public Veterinary2()
        {
            reference = "";
            type = "";
            imageUrl = "";
            email = "";
            username = "";
            password = "";
            idcard = "";
            lineID = "";
            position = "";
            prefixName = "";
            firstName = "";
            lastName = "";
            firstNameEN = "";
            lastNameEN = "";

            oldPosition = "";
            oldPrefixName = "";
            oldFirstName = "";
            oldLastName = "";
            oldFirstNameEN = "";
            oldLastNameEN = "";

            birthDay = "";
            age = "";
            nationality = "";
            race = "";
            religion = "";

            phone = "";
            telephone = "";
            soi = "";
            address = "";
            address2 = "";
            moo = "";
            road = "";
            tambon = "";
            amphoe = "";
            province = "";
            postno = "";

            isWorkSameHome = false;
            workPhone = "";
            workSoi = "";
            workMoo = "";
            workAddress = "";
            workAddress2 = "";
            workRoad = "";
            workTambon = "";
            workAmphoe = "";
            workProvince = "";
            workPostno = "";

            workType = "";
            workTypeDescription = "";

            isCurrentSameHome = false;
            isCurrentSameWork = false;
            currentPhone = "";
            currentSoi = "";
            currentMoo = "";
            currentAddress = "";
            currentAddress2 = "";
            currentRoad = "";
            currentTambon = "";
            currentAmphoe = "";
            currentProvince = "";
            currentPostno = "";

            education = "";
            educationDegree = "";
            educationYear = "";
            secondaryEducation = "";
            secondaryEducationYear = "";
            note = "";
            dateStart = "";
            dateEnd = "";
            licenseNumber = "";
        }

        public string reference { get; set; }
        public string type { get; set; }  // 1 = user update, 2 = admin update.
        public string imageUrl { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string idcard { get; set; }
        public string lineID { get; set; }
        public string position { get; set; }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string firstNameEN { get; set; }
        public string lastNameEN { get; set; }

        public string oldPosition { get; set; }
        public string oldPrefixName { get; set; }
        public string oldFirstName { get; set; }
        public string oldLastName { get; set; }
        public string oldFirstNameEN { get; set; }
        public string oldLastNameEN { get; set; }

        public string birthDay { get; set; }
        public string age { get; set; }
        public string nationality { get; set; }
        public string race { get; set; }
        public string religion { get; set; }

        public string phone { get; set; }
        public string telephone { get; set; }
        public string soi { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string moo { get; set; }
        public string road { get; set; }
        public string tambon { get; set; }
        public string amphoe { get; set; }
        public string province { get; set; }
        public string postno { get; set; }

        public bool isWorkSameHome { get; set; }
        public string workPhone { get; set; }
        public string workSoi { get; set; }
        public string workAddress { get; set; }
        public string workAddress2 { get; set; }
        public string workMoo { get; set; }
        public string workRoad { get; set; }
        public string workTambon { get; set; }
        public string workAmphoe { get; set; }
        public string workProvince { get; set; }
        public string workPostno { get; set; }

        public string workType { get; set; }
        public string workTypeDescription { get; set; }

        public bool isCurrentSameHome { get; set; }
        public bool isCurrentSameWork { get; set; }
        public string currentPhone { get; set; }
        public string currentSoi { get; set; }
        public string currentAddress { get; set; }
        public string currentAddress2 { get; set; }
        public string currentMoo { get; set; }
        public string currentRoad { get; set; }
        public string currentTambon { get; set; }
        public string currentAmphoe { get; set; }
        public string currentProvince { get; set; }
        public string currentPostno { get; set; }

        public string education { get; set; }
        public string educationDegree { get; set; }
        public string educationYear { get; set; }
        public string secondaryEducation { get; set; }
        public string secondaryEducationYear { get; set; }
        public string note { get; set; }
        public string dateStart { get; set; }
        public string dateEnd { get; set; }
        public string licenseNumber { get; set; }
    }
}
