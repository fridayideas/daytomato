using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DayTomato.Models
{

	[JsonConverter(typeof(PinConverter))]
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

		public string Id { get; set; }

		// Type of pin depends on the amount of seeds the user has
		// 0-10 seeds normal user; 10-50 seeds, ...
		public int Type { get; set; }
		public string Name { get; set; }
		public float Rating { get; set; }
		public string Description { get; set; }
		public int Likes { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string LinkedAccount { get; set; }
		public string Review { get; set;}
        public double Cost { get; set; }
		public List<Comment> Comments { get; set; }
		public DateTime CreateDate { get; set; }
		public string ImageURL { get; set; }

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
				pin.Id = (string)jo["_id"];                                // Id of pin
				pin.Type = (int)jo["pinType"];                             // Type of pin
				pin.Name = (string)jo["name"];                       	   // Name of pin
				pin.Rating = (float)jo["rating"];                          // Rating of pin
				pin.Description = (string)jo["description"];               // Description of pin
				pin.Likes = (int)jo["likes"];                              // Pin likes
				pin.Review = (string)jo["review"];						   // Pin review
                pin.Cost = (double)jo["cost"];                             // Pin cost
				pin.Latitude = (double)jo["coordinate"]["latitude"];       // Pin latitude
				pin.Longitude = (double)jo["coordinate"]["longitude"];     // Pin longitude
				pin.LinkedAccount = (string)jo["linkedAccount"];           // Pin linked account
				//pin.CreateDate = (DateTime)jo["createDate"];               // Pin create date
				pin.ImageURL = (string)jo["image"];						   // Pin image url
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			/* Comments
             * Comments are in this format:
             * [{"linkedAccount":111,"text":"LOOOOL","createDate":"2016-09-30T02:44:20.637Z"}]
             */
			/*try
			{ 
				pin.Comments = new List<Comment>();
				JArray ja = (JArray)jo["comments"];
				for (int i = 0; i < ja.Count; ++i)
				{
					string account = (string)ja[i]["linkedAccount"];
					string text = (string)ja[i]["text"];
					DateTime date = (DateTime)ja[i]["createDate"];
					pin.Comments.Add(new Comment(account, text, date));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}*/	

			return pin;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject jo = new JObject();

			Pin pin = (Pin)value;
			jo.Add("_id", pin.Id);
			jo.Add("pinType", pin.Type);
			jo.Add("name", pin.Name);
			jo.Add("rating", pin.Rating);
			jo.Add("description", pin.Description);
			JObject coordinates = new JObject();
			coordinates.Add("latitude", pin.Latitude);
			coordinates.Add("longitude", pin.Longitude);
			jo.Add("coordinate", coordinates);
			jo.Add("linkedAccount", pin.LinkedAccount);
			jo.Add("likes", pin.Likes);
			jo.Add("review", pin.Review);
            jo.Add("cost", pin.Cost);
			jo.Add("image", pin.ImageURL);
			jo.Add("createDate", pin.CreateDate);

			JArray ja = new JArray();
			if (pin.Comments != null)
			{
				for (int i = 0; i < pin.Comments.Count; ++i)
				{
					JObject jr = new JObject();
					jr.Add("linkedAccount", pin.Comments[i].LinkedAccount);
					jr.Add("text", pin.Comments[i].Text);
					jr.Add("createDate", pin.Comments[i].CreateDate);
					ja.Add(jr);
				}
			}

			jo.Add("comments", ja);

			jo.WriteTo(writer);
		}
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
