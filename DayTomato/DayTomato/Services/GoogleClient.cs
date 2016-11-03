using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DayTomato
{
	public class GoogleClient
	{
		HttpClient httpClient;
		private readonly string GOOGLE_API_KEY = "AIzaSyDU2aOZLIaBsZ4s62PQ1T88e9UL0QvLsoA";
		private readonly string GOOGLE_PLACES_BASE_URL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
		private readonly string GOOGLE_PLACES_PHOTO_BASE_URL = "https://maps.googleapis.com/maps/api/place/photo?";
		private readonly string GOOGLE_RANK_BY = "distance";
		private readonly string GOOGLE_PHOTO_MAX_WIDTH = "256";

		public GoogleClient()
		{
			httpClient = new HttpClient();
			httpClient.MaxResponseContentBufferSize = 256000;
		}

		// Get Place
		public async Task<Place> GetPlace(double lat, double lng)
		{
			Place place = new Place();
			string result = "";
			var uri = new Uri(GOOGLE_PLACES_BASE_URL
							  + "location=" + lat + "," + lng
							  + "&rankby=" + GOOGLE_RANK_BY
							  + "&key=" + GOOGLE_API_KEY);
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				result = await response.Content.ReadAsStringAsync();
				place = JsonConvert.DeserializeObject<Place>(result);
			}

			uri = new Uri(GOOGLE_PLACES_PHOTO_BASE_URL
						  + "maxwidth=" + GOOGLE_PHOTO_MAX_WIDTH
						  + "&photoreference=" + place.PhotoReference
						  + "&key=" + GOOGLE_API_KEY);
			response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				place.Image = await response.Content.ReadAsByteArrayAsync();
			}

			return place;
		}
	}
}
