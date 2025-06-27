using System;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Portfolio : Identity
	{
		public Portfolio()
		{
            status2 = "";
            imageUrl = "";
            imageBanner = "";
            imageExample = "";
            imageUrlCreateBy = "";
            view = 0;
            progress = 0;
            functionList = "";
        }

        public string status2 { get; set; }
        public string imageUrl { get; set; }
        public string imageBanner { get; set; }
        public string imageExample { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public int progress { get; set; }
        public String functionList { get; set; }
    }

    public class PortfolioFunction
    {
        public PortfolioFunction()
        {
            title = "";
            description = "";
        }

        public string title { get; set; }
        public string description { get; set; }
    }
}

