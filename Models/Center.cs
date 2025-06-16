using System;
namespace cms_api.Models
{
    public class Center : Identity
    {
        public Center()
        {
            imageUrl = "";
            reference = "";
            email = "";
            name = "";
            phone = "";
            license = "";
            emailCenter = "";
        }

        public string imageUrl { get; set; }
        public string reference { get; set; }
        public bool isMenu { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string license { get; set; }
        public string emailCenter { get; set; }

        internal object FirstOrDefault()
        {
            throw new NotImplementedException();
        }
    }

}
