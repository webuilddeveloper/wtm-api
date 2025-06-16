using System;
namespace cms_api.Models
{
    public class Gallery : Identity
    {
        public Gallery()
        {
            imageUrl = "";
            type = "";
            reference = "";
            size = "";
        }

        public string imageUrl { get; set; }
        public string type { get; set; }
        public string reference { get; set; }
        public string size { get; set; }
    }
}
