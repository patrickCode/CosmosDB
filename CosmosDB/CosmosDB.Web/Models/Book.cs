using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CosmosDB.Web.Models
{
    public class Book
    {
        public Book() { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public int SerialNumber { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Price { get; set; }
        public List<Review> Reviews { get; set; }
        public bool IsRead { get; set; }
    }
}