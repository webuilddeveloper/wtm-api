using System;
namespace cms_api.Models
{
    public class OfficeActivities : Identity
    {
        public OfficeActivities()
        {
            imageUrl = "";
        }

        public string imageUrl { get; set; }
    }
}
