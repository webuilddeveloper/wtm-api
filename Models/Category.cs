using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class Category : Identity
    {
        public Category()
        {
            imageUrl = "";
            reference = "";
        }

        public string imageUrl { get; set; }
        public string reference { get; set; }
        public List<Category> items { get; set; }
    }
}
