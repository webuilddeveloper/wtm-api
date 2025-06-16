using System;
namespace cms_api.Models
{
    public class Banner : Identity
    {
        public Banner()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            note = "";
            isPostHeader = false;
        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string note { get; set; }
        public bool isPostHeader { get; set; }
    }
}
