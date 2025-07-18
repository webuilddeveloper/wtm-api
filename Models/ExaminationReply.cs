﻿using System;
namespace cms_api.Models
{
    public class ExaminationReply : Identity
    {
        public ExaminationReply()
        {
            answer = "";
            username = "";
            firstName = "";
            lastName = "";
            points = 0;
            reference = "";
            imageUrlCreateBy = "";
            keySearch = "";
            skip = 0;
            limit = 0;
            startDate = "";
            endDate = "";
        }

        public string answer { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int points { get; set; }
        public string reference { get; set; }
        public string imageUrlCreateBy { get; set; }
       
        public string keySearch { get; set; }
        public int skip { get; set; }
        public int limit { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
}
