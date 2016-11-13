using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DayTomato.Models
{
	public class Pin
	{
		public Pin() { }

		public Pin(string id,
				   int type, 
		           string name, 
		           float rating, 
		           string description, 
		           int likes, 
		           double lat, 
		           double lng, 
		           string account,
		           string username,
		           string review,
		           double cost,
		           DateTime date,
		           string imageURL)
		{
			Id = id;
			Type = type;
			Name = name;
			Rating = rating;
			Description = description;
			Likes = likes;
			Coordinate = new Coordinate(lat, lng);
			LinkedAccount = account;
			Username = username;
			Review = review;
			Cost = cost;
			Comments = new List<Comment>();
			CreateDate = date;
			ImageURL = imageURL;
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
		[JsonProperty("username")]
		public string Username { get; set; }
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
	
		public int GuessType(string type)
		{
			type = type.ToLower();
			string[] types = type.Split(' ');
			for (int i = 0; i < types.Length; i++)
			{
				foreach (string t in Enum.GetNames(typeof(Models.FOOD_TYPES)))
				{
					if (t.Equals(types[i]))
					{
						return 1;
					}
				}
				foreach (string t in Enum.GetNames(typeof(Models.POI_TYPES)))
				{
					if (t.Equals(types[i]))
					{
						return 2;
					}
				}
				foreach (string t in Enum.GetNames(typeof(Models.SHOPPING_TYPES)))
				{
					if (t.Equals(types[i]))
					{
						return 3;
					}
				}
				foreach (string t in Enum.GetNames(typeof(Models.OUTDOOR_TYPES)))
				{
					if (t.Equals(types[i]))
					{
						return 4;
					}
				}
			}

			// General
			return 0;
		}
	}

	public class Comment
	{
		public Comment(string account, string username, string text, DateTime date)
		{
			LinkedAccount = account;
			Username = username;
			Text = text;
			CreateDate = date;
		}

		public string LinkedAccount { get; set; }
		public string Username { get; set; }
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

	public enum GENERAL_TYPES
	{
		airport,
		atm,
		bank,
		bus_station,
		cemetery,
		courthouse,
		dentist,
		doctor,
		electrician,
		fire_station,
		funeral_home,
		gym,
		hospital,
		insurance_agency,
		laundry,
		lawyer,
		lodging,
		moving_company,
		painter,
		physiotherapist,
		plumber,
		police,
		post_office,
		real_estate_agency,
		roofing_contractor,
		storage,
		subway_station,
		taxi_stand,
		train_station,
		transit_station,
		travel_agency,
		veterinary_care
	}

	public enum POI_TYPES
	{
		amusement_park,
		aquarium,
		art_gallery,
		bowling_alley,
		casino,
		church,
		city_hall,
		embassy,
		hindu_temple,
		library,
		local_government_office,
		mosque,
		museum,
		night_club,
		school,
		synagogue,
		university,
		zoo,
		performing,
		arts,
		cinema,
		art,
		museums,
		botanical,
		gardens,
		wineries
	}

	public enum SHOPPING_TYPES
	{
		accounting,
		beauty_salon,
		bicycle_store,
		book_store,
		car_dealer,
		car_rental,
		car_repair,
		car_wash,
		clothing_store,
		convenience_store,
		department_store,
		electronics_store,
		florist,
		furniture_store,
		gas_station,
		hair_care,
		hardware_store,
		home_goods_store,
		jewelry_store,
		locksmith,
		movie_rental,
		movie_theater,
		pet_store,
		pharmacy,
        store,
		shoe_store,
        shopping,
		shopping_mall,
		spa
	}

	public enum OUTDOOR_TYPES
	{
		campground,
		park,
		parking,
		rv_park,
		stadium
	}

	public enum FOOD_TYPES
	{
		bakery,
		bar,
		cafe,
		liquor_store,
		meal_delivery,
		meal_takeaway,
		restaurant,
		coffee,
		tea,
		pubs,
		bubble,
		bbq,
		meat,
		baked,
		vietnamese,
		canadian,
		delis,
		breakfast,
		lunch,
		dinner,
		burgers,
		brunch,
		ramen,
		mediterranean,
		japanese,
		pizza,
		asian,
		steakhouse,
		salad,
		chinese,
		thai,
		mexican
	}
}
