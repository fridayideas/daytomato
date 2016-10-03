using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DayTomato.Models
{
	/*
    {
        "_id": <ObjectId>
        "rating": <string>,
        "description": <string>, 
        "likes" : <int>,
        "coordinate": {
            "latitude": <double>,
            "longitude": <double>
        },
        "linkedAccount": <ObjectId>, 
        "reviews": <List>
    }
    */

	[JsonConverter(typeof(PinConverter))]
	public class Pin
	{
		private string _id;
		private int _type;
		private string _name;
		private float _rating;
		private string _description;
		private int _likes;
		private double _latitude;
		private double _longitude;
		private string _linkedAccount;
		private List<Review> _reviews;
		private DateTime _createDate;

		public Pin() { }

		public Pin(int type, string name, float rating, string description, int likes, double lat, double lng, string account, DateTime date)
		{
			_type = type;
			_name = name;
			_rating = rating;
			_description = description;
			_likes = likes;
			_latitude = lat;
			_longitude = lng;
			_linkedAccount = account;
			_reviews = new List<Review>();
			_createDate = date;
		}

		public string Id { get { return _id; } set { _id = value; } }

		// Type of pin depends on the amount of seeds the user has
		// 0-10 seeds normal user; 10-50 seeds, ...
		public int Type { get { return _type; } set { _type = value; } }
		public string Name { get { return _name; } set { _name = value; } }
		public float Rating { get { return _rating; } set { _rating = value; } }
		public string Description { get { return _description; } set { _description = value; } }
		public int Likes { get { return _likes; } set { _likes = value; } }
		public double Latitude { get { return _latitude; } set { _latitude = value; } }
		public double Longitude { get { return _longitude; } set { _longitude = value; } }
		public string LinkedAccount { get { return _linkedAccount; } set { _linkedAccount = value; } }
		public List<Review> Reviews { get { return _reviews; } set { _reviews = value; } }
		public DateTime CreateDate { get { return _createDate; } set { _createDate = value; } }

	}

	public class PinConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(Pin));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			Pin pin = new Pin();
			try
			{
				pin.Id = (string)jo["_id"];                             // Id of pin
				pin.Type = (int)jo["pinType"];                          // Type of pin
				pin.Name = (string)jo["pinName"];                       // Name of pin
				pin.Rating = (int)jo["rating"];                         // Rating of pin
				pin.Description = (string)jo["description"];            // Description of pin
				pin.Likes = (int)jo["likes"];                           // Pin likes
				pin.Latitude = (double)jo["coordinate"]["latitude"];    // Pin latitude
				pin.Longitude = (double)jo["coordinate"]["longitude"];  // Pin longitude
				pin.LinkedAccount = (string)jo["linkedAccount"];        // Pin linked account
				pin.CreateDate = (DateTime)jo["createDate"];            // Pin create date
			}
			catch
			{
				// TODO:Catch error here
			}

			/* Rewviews
             * Reviews are in this format:
             * [{"linkedAccount":111,"text":"LOOOOL","createDate":"2016-09-30T02:44:20.637Z"}]
             */
			JArray ja = (JArray)jo["reviews"];
			for (int i = 0; i < ja.Count; ++i)
			{
				string reviewAccount = (string)ja[i]["linkedAccount"];
				string reviewText = (string)ja[i]["text"];
				DateTime reviewCreateDate = (DateTime)ja[i]["createDate"];
				pin.Reviews = new List<Review>();
				pin.Reviews.Add(new Review(reviewAccount, reviewText, reviewCreateDate));
			}
			if (ja.Count == 0)
			{
				pin.Reviews = new List<Review>();
				pin.Reviews.Add(new Review(pin.LinkedAccount, "No Reviews", pin.CreateDate));
			}

			return pin;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject jo = new JObject();

			Pin pin = (Pin)value;

			jo.Add("pinType", pin.Type);
			jo.Add("pinName", pin.Name);
			jo.Add("rating", pin.Rating);
			jo.Add("description", pin.Description);
			JObject coordinates = new JObject();
			coordinates.Add("latitude", pin.Latitude);
			coordinates.Add("longitude", pin.Longitude);
			jo.Add("coordinate", coordinates);
			jo.Add("linkedAccount", pin.LinkedAccount);
			jo.Add("likes", pin.Likes);

			JArray ja = new JArray();
			if (pin.Reviews != null)
			{
				for (int i = 0; i < pin.Reviews.Count; ++i)
				{
					JObject jr = new JObject();
					jr.Add("linkedAccount", pin.Reviews[i].LinkedAccount);
					jr.Add("text", pin.Reviews[i].Text);
					jr.Add("createDate", pin.Reviews[i].CreateDate);
					ja.Add(jr);
				}
			}

			jo.Add("reviews", ja);

			jo.WriteTo(writer);
		}
	}

	public class Review
	{
		private string _linkedAccount;
		private string _text;
		private DateTime _createDate;
		public Review(string account, string text, DateTime date)
		{
			_linkedAccount = account;
			_text = text;
			_createDate = date;
		}

		public string LinkedAccount { get {return _linkedAccount;} set {_linkedAccount = value;} }
		public string Text { get { return _text; } set { _text = value;} }
		public DateTime CreateDate { get { return _createDate; } set { _createDate = value;} }
    }
}
