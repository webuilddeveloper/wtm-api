using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class RelateAgency : Identity
    {
        public RelateAgency()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            note = "";
            isPostHeader = false;
            imageSize = "";
            reference = "";
            relateagencyitems = new List<RelateAgency>();


        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string note { get; set; }
        public bool isPostHeader { get; set; }
        public string imageSize { get; set; }
        public string reference { get; set; }
        public List<RelateAgency> relateagencyitems { get; set; }

    }
}
