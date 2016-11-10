using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DayTomato
{
	public class GoogleClient
	{
		HttpClient httpClient;
		private readonly string GOOGLE_API_KEY = "AIzaSyDU2aOZLIaBsZ4s62PQ1T88e9UL0QvLsoA";
		private readonly string GOOGLE_PLACES_AUTO_COMPLETE_BASE_URL = "https://maps.googleapis.com/maps/api/place/autocomplete/json?";
		private readonly string GOOGLE_GEOCODING_BASE_URL = "https://maps.googleapis.com/maps/api/geocode/json?";
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

		public async Task<string[]> PredictPlaces(string input , double lat, double lng)
		{
			string[] predictions;
			// Example:
			// https://maps.googleapis.com/maps/api/place/autocomplete/xml?
			// input=Amoeba&types=establishment&location=37.76999,-122.44696&radius=500&key=YOUR_API_KEY

			var uri = new Uri(GOOGLE_PLACES_AUTO_COMPLETE_BASE_URL
							  + "input=" + input
							  + "&location=" + lat + "," + lng
			                  + "&radius=20000"
							  + "&key=" + GOOGLE_API_KEY);

			var result = "";
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				result = await response.Content.ReadAsStringAsync();
				var Jsonobject = JsonConvert.DeserializeObject<Place.RootObject>(result);
				List<Place.Prediction> googlePredictions = Jsonobject.predictions;
				predictions = new string[googlePredictions.Count];

				int index = 0;
				foreach (Place.Prediction p in googlePredictions)
				{
					predictions[index] = p.description;
					index++;
				}
				return predictions;
			}

			return new string[] { };
		}

        public async Task<string[]> PredictCities(string input)
        {
            string[] predictions;
            // Example:
            // https://maps.googleapis.com/maps/api/place/autocomplete/xml?
            // input=Amoeba&types=establishment&location=37.76999,-122.44696&radius=500&key=YOUR_API_KEY

            var uri = new Uri(GOOGLE_PLACES_AUTO_COMPLETE_BASE_URL
                              + "input=" + input
                              + "&types=(cities)"
                              + "&key=" + GOOGLE_API_KEY);

            var result = "";
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
                var Jsonobject = JsonConvert.DeserializeObject<Place.RootObject>(result);
                List<Place.Prediction> googlePredictions = Jsonobject.predictions;
                predictions = new string[googlePredictions.Count];

                int index = 0;
                foreach (Place.Prediction p in googlePredictions)
                {
                    predictions[index] = p.description;
                    index++;
                }
                return predictions;
            }

            return new string[] { };
        }

        public async Task<Models.Coordinate> Geocode(string address)
		{
			Models.Coordinate coords = new Models.Coordinate(0,0);
			// Example:
			// https://maps.googleapis.com/maps/api/geocode/json?
			// address=1600+Amphitheatre+Parkway,+Mountain+View,+CA&key=YOUR_API_KEY

			var uri = new Uri(GOOGLE_GEOCODING_BASE_URL
							  + "address=" + address
							  + "&key=" + GOOGLE_API_KEY);

			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsStringAsync();
				var geocode = JsonConvert.DeserializeObject<Geocode.RootObject>(result);

				coords.latitude = geocode.results[0].geometry.location.lat;
				coords.longitude = geocode.results[0].geometry.location.lng;
			}

			return coords;
		}
	}
}
