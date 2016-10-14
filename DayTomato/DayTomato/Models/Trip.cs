using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DayTomato.Models
{
	[JsonConverter(typeof(TripConverter))]
    public class Trip
    {
		public string Id { get; set; }
        public List<Pin> Pins { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string LinkedAccount { get; set; }
		public DateTime CreateDate { get; set; }
    }

	public class TripConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(Trip));
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			Trip trip = new Trip();
			try
			{
				trip.Id = (string)jo["_id"];                                // Id of trip
				trip.Name = (string)jo["name"];                             // Type of trip
				trip.Type = (string)jo["type"];                             // Name of trip
				trip.LinkedAccount = (string)jo["linkedAccount"];           // Rating of trip
				trip.CreateDate = (DateTime)jo["createDate"];               // Description of trip
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			try
			{
				trip.Pins = new List<Pin>();
				JArray ja = (JArray)jo["pins"];
				for (int i = 0; i < ja.Count; ++i)
				{
					Pin pin = JsonConvert.DeserializeObject<Pin>(ja[i].ToString());
					trip.Pins.Add(pin);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			return trip;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject jo = new JObject();

			Trip trip = (Trip)value;
			jo.Add("_id", trip.Id);
			jo.Add("type", trip.Type);
			jo.Add("name", trip.Name);
			jo.Add("linkedAccount", trip.LinkedAccount);
			jo.Add("createDate", trip.CreateDate);

			JArray ja = new JArray();
			if (trip.Pins != null)
			{
				for (int i = 0; i < trip.Pins.Count; ++i)
				{
					JObject jr = new JObject();
					jr = JObject.Parse(JsonConvert.SerializeObject(trip.Pins[i]));
					ja.Add(jr);
				}
			}

			jo.Add("pins", ja);

			jo.WriteTo(writer);
		}
	}
}
