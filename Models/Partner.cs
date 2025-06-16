using System;
namespace cms_api.Models
{
    public class Partner : Identity
    {
        public Partner()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            note = "";
            isPostHeader = false;
            imageSize = "";

        }

        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string note { get; set; }
        public bool isPostHeader { get; set; }
        public string imageSize { get; set; }
    }
}
