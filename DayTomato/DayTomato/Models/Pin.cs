using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DayTomato.Models
{
	public class Pin
	{
		public Pin() { }

		public Pin(int type, string name, float rating, double cost, string description, int likes, double lat, double lng, string account, DateTime date)
		{
			Type = type;
			Name = name;
			Rating = rating;
            Cost = cost;
			Description = description;
			Likes = likes;
			Latitude = lat;
			Longitude = lng;
			LinkedAccount = account;
			Comments = new List<Comment>();
			CreateDate = date;
		}

		[JsonProperty("_id")]
		public string Id { get; set; }
		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Type { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("rating", DefaultValueHandling = DefaultValueHandling.Populate) ]
		public float Rating { get; set; }
		[JsonProperty("description")]
		public string Description { get; set; }
		[JsonProperty("likes", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Likes { get; set; }
		[JsonProperty("latitude", DefaultValueHandling = DefaultValueHandling.Populate)]
		public double Latitude { get; set; }
		[JsonProperty("longitude", DefaultValueHandling = DefaultValueHandling.Populate)]
		public double Longitude { get; set; }
		[JsonProperty("linkedAccount")]
		public string LinkedAccount { get; set; }
		[JsonProperty("review")]
		public string Review { get; set;}
		[JsonProperty("cost", DefaultValueHandling = DefaultValueHandling.Populate)]
        public double Cost { get; set; }
		[JsonProperty("comments", TypeNameHandling = TypeNameHandling.Auto)]
		public List<Comment> Comments { get; set; }
		[JsonProperty("createDate")]
		public DateTime CreateDate { get; set; }
		[JsonProperty("image")]
		public string ImageURL { get; set; }
	}

	public class Comment
	{
		public Comment(string account, string text, DateTime date)
		{
			LinkedAccount = account;
			Text = text;
			CreateDate = date;
		}

		public string LinkedAccount { get; set; } 
		public string Text { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
