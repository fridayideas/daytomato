using System;
using DayTomato.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DayTomato
{
	[JsonConverter(typeof(PlaceConverter))]
	public class Place
	{
		public string Name { get; set; }
		public string PhotoReference { get; set; }
		public Byte[] Image { get; set; }
		public string Description { get; set; }
		public string[] Types { get; set; }

		public Place()
		{
		}

		public class PlaceConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return (objectType == typeof(Place));
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var jo = JObject.Load(reader);
				var place = new Place();

				// For each result, find the result that contains all required info
				var ja = (JArray)jo["results"];
				for (int i = 0; i < ja.Count; ++i)
				{
					try
					{
						place.Name = (string)ja[i]["name"];
						var jat = (JArray)ja[i]["types"];
						place.Description = "";
						for (int j = 0; j < jat.Count; ++j)
						{
							place.Description += ((string)jat[j]).Replace('_', ' ');
							place.Types = (string[])jat[j].Values<string>();
						}
						var jap = (JArray)ja[i]["photos"];
						for (int j = 0; j < jap.Count; ++j)
						{
							place.PhotoReference = (string)jap[j]["photo_reference"];
							if (place.Image != null) break;
						}
					}
					catch (Exception ex)
					{ }

					if (place.Name != null && place.Image != null) break;
				}

				return place;
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}
}
