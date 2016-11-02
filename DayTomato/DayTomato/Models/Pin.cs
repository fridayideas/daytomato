using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;

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
			Coordinate = new Coordinate(lat, lng);
			LinkedAccount = account;
			Comments = new List<Comment>();
			CreateDate = date;
			LikedBy = new List<string>();
			DislikedBy = new List<string>();
		}

		[JsonProperty("_id")]
		public string Id { get; set; }
		[JsonProperty("pinType", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Type { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("rating", DefaultValueHandling = DefaultValueHandling.Populate)]
		public float Rating { get; set; }
		[JsonProperty("description")]
		public string Description { get; set; }
		[JsonProperty("likes", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Likes { get; set; }
		[JsonProperty("coordinate", TypeNameHandling = TypeNameHandling.Auto)]
		public Coordinate Coordinate { get; set; }
		[JsonProperty("linkedAccount")]
		public string LinkedAccount { get; set; }
		[JsonProperty("review")]
		public string Review { get; set; }
		[JsonProperty("cost", DefaultValueHandling = DefaultValueHandling.Populate)]
		public double Cost { get; set; }
		[JsonProperty("comments", TypeNameHandling = TypeNameHandling.Auto)]
		public List<Comment> Comments { get; set; }
		[JsonProperty("createDate")]
		public DateTime CreateDate { get; set; }
		[JsonProperty("image")]
		public string ImageURL { get; set; }
		[JsonProperty("likedBy")]
		public List<string> LikedBy { get; set; }
		[JsonProperty("dislikedBy")]
		public List<string> DislikedBy { get; set; }
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

	public class Coordinate
	{
		public Coordinate(double lat, double lng)
		{
			latitude = lat;
			longitude = lng;
		}
		public double latitude { get; set; }
		public double longitude { get; set; }
	}
}
