﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DayTomato.Models
{
    public class Trip
    {
        [JsonProperty("_id")]
		public string Id { get; set; }
        [JsonProperty("pins")]
        public List<Pin> Pins { get; set; }
        [JsonProperty("name")]
		public string Name { get; set; }
        [JsonProperty("type")]
		public string Type { get; set; }
        [JsonProperty("linkedAccount")]
		public string LinkedAccount { get; set; }
		[JsonProperty("linkedAccountId")]
		public string LinkedAccountId { get; set; }
        [JsonProperty("createDate")]
		public DateTime CreateDate { get; set; }
        [JsonProperty("likes", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Likes { get; set; }
        [JsonProperty("description")]
		public string Description { get; set; }
        [JsonProperty("cost", DefaultValueHandling = DefaultValueHandling.Populate)]
		public double Cost { get; set; }
        [JsonProperty("rating", DefaultValueHandling = DefaultValueHandling.Populate)]
		public float Rating { get; set; }
        [JsonProperty("comments", TypeNameHandling = TypeNameHandling.Auto)]
		public List<Comment> Comments { get; set; }
		[JsonProperty("likedBy")]
		public List<string> LikedBy { get; set; }
		[JsonProperty("dislikedBy")]
		public List<string> DislikedBy { get; set; }

		public Trip() { }

		public Trip(CreateTrip trip, List<Pin> pins)
		{
			Id = trip.Id;
			Pins = pins;
			Name = trip.Name;
			Type = trip.Type;
			LinkedAccount = trip.LinkedAccount;
			LinkedAccountId = trip.LinkedAccountId;
			CreateDate = trip.CreateDate;
			Likes = trip.Likes;
			Description = trip.Description;
			Cost = trip.Cost;
			Rating = trip.Rating;
			Comments = trip.Comments;
			LikedBy = trip.LikedBy;
			DislikedBy = trip.DislikedBy;
		}
    }
}
