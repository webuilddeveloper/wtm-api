using System;
namespace cms_api.Models
{
    public class Organization
    {
        public Organization()
        {
            status = "";
            lv0 = "";
            lv1 = "";
            lv2 = "";
            lv3 = "";
            lv4 = "";
            profileCode = "";
            platform = "";
        }

        public string status { get; set; }
        public string lv0 { get; set; }
        public string lv1 { get; set; }
        public string lv2 { get; set; }
        public string lv3 { get; set; }
        public string lv4 { get; set; }
        public string profileCode { get; set; }
        public string platform { get; set; }
    }
}
