﻿using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class Law : Identity
    {
        public Law()
        {
            imageUrl = "";
            imageUrlCreateBy = "";
            author = "";
            publisher = "";
            bookType = "";
            numberOfPages = 0;
            size = "";
            publishDate = "";
            view = 0;
            categoryCardType = "";
        }

        public int view { get; set; }
        public string imageUrl { get; set; }
        public string imageUrlCreateBy { get; set; }
        public string author { get; set; }
        public string publisher { get; set; }
        public string bookType { get; set; }
        public int numberOfPages { get; set; }
        public string size { get; set; }
        public string publishDate { get; set; }
        public string categoryCardType { get; set; }
    }
}
