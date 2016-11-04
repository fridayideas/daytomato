using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public List<string> Types { get; set; }

		public int PlaceType { get; set; }

		public int GuessType(string type)
		{
			foreach (string t in Enum.GetNames(typeof(Models.FOOD_TYPES)))
			{
				if (t.Equals(type))
				{
					return 1;
				}
			}
			foreach (string t in Enum.GetNames(typeof(Models.POI_TYPES)))
			{
				if (t.Equals(type))
				{
					return 2;
				}
			}
			foreach (string t in Enum.GetNames(typeof(Models.SHOPPING_TYPES)))
			{
				if (t.Equals(type))
				{
					return 3;
				}
			}
			foreach (string t in Enum.GetNames(typeof(Models.OUTDOOR_TYPES)))
			{
				if (t.Equals(type))
				{
					return 4;
				}
			}

			// General
			return 0;
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
						// Get the name
						place.Name = (string)ja[i]["name"];

						// Get the description and types
						var jat = (JArray)ja[i]["types"];
						place.Description = "";
						place.Types = new List<string>();
						bool first = true;
						for (int j = 0; j < jat.Count; ++j)
						{
							string type = jat[j].Value<string>();
							if (first)
							{
								place.PlaceType = place.GuessType(type);
								first = false;
							}
							place.Types.Add(type);
							place.Description += (type).Replace('_', ' ') + " ";
						}

						// Get the photo
						var jap = (JArray)ja[i]["photos"];
						for (int j = 0; j < jap.Count; ++j)
						{
							place.PhotoReference = (string)jap[j]["photo_reference"];
							if (place.PhotoReference != null) break;
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}

					if (place.Name != null) break;
				}

				return place;
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}

		// Google Places JSON To C#
		public class MatchedSubstring
		{
			[JsonProperty(PropertyName = "length")]
			public int length { get; set; }
			[JsonProperty(PropertyName = "offset")]
			public int offset { get; set; }
		}

		public class Term
		{
			[JsonProperty(PropertyName = "offset")]
			public int offset { get; set; }
			[JsonProperty(PropertyName = "value")]
			public string value { get; set; }
		}

		public class Prediction
		{
			[JsonProperty(PropertyName = "description")]
			public string description { get; set; }
			[JsonProperty(PropertyName = "id")]
			public string id { get; set; }
			[JsonProperty(PropertyName = "matched_substrings")]
			public List<MatchedSubstring> matched_substrings { get; set; }
			[JsonProperty(PropertyName = "reference")]
			public string reference { get; set; }
			[JsonProperty(PropertyName = "terms")]
			public List<Term> terms { get; set; }
			[JsonProperty(PropertyName = "types")]
			public List<string> types { get; set; }
		}

		public class RootObject
		{
			[JsonProperty(PropertyName = "predictions")]
			public List<Prediction> predictions { get; set; }
			[JsonProperty(PropertyName = "status")]
			public string status { get; set; }
		}
		// Google Places JSON To C#
	}
}
