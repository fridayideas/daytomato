using DayTomato.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace DayTomato.Services
{
    public class DayTomatoClient
    {
        HttpClient httpClient;

#if DEBUG
        private readonly string BASE_URL = "http://fridayideas.herokuapp.com";
#else
		private readonly string BASE_URL = "http://fridayideas.herokuapp.com";
#endif

        private readonly string GOOGLE_API_KEY = "AIzaSyDU2aOZLIaBsZ4s62PQ1T88e9UL0QvLsoA";
		private readonly string GOOGLE_PLACES_BASE_URL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
		private readonly string GOOGLE_PLACES_PHOTO_BASE_URL = "https://maps.googleapis.com/maps/api/place/photo?";
		private readonly string GOOGLE_RANK_BY = "distance";
		private readonly string GOOGLE_PHOTO_MAX_WIDTH = "256";

		private readonly string IMGUR_BASE_URL = "https://api.imgur.com/3";
		private readonly string IMGUR_CLIENT_ID = "1f30123ee30a53b";
		private readonly string IMGUR_CLIENT_SECRET = "979732009beba54d18e67f6dc9f8c3fa79082d16";

        public DayTomatoClient()
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
        }

        public DayTomatoClient(string idToken)
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
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

		// Get All Trips
		public async Task<List<Trip>> GetTrips()
		{
			List<Trip> trips = new List<Trip>();
			var uri = new Uri(BASE_URL + "/api/trips");
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				string content = await response.Content.ReadAsStringAsync();
				trips = JsonConvert.DeserializeObject<List<Trip>>(content);
				return trips;
			}

			return null;
		}

        // Get Pins
        public async Task<List<Pin>> GetPins()
        {
            List<Pin> pins = new List<Pin>();
            var uri = new Uri(BASE_URL + "/api/pins");
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                pins = JsonConvert.DeserializeObject<List<Pin>>(content);
                return pins;
            }

            return null;
        }

		// Get Pins In Area
		public async Task<List<Pin>> GetPins(double latTopLeft, double lngTopLeft, double latBotRight, double lngBotRight)
		{
			List<Pin> pins = new List<Pin>();
			var uri = new Uri(BASE_URL + "/api/pins?searcharea=" + Convert.ToString(latTopLeft) + "," 
			                  									 + Convert.ToString(lngTopLeft) + ","
			                 									 + Convert.ToString(latBotRight) + ","
			                 									 + Convert.ToString(lngBotRight));
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				string content = await response.Content.ReadAsStringAsync();
				pins = JsonConvert.DeserializeObject<List<Pin>>(content);
				return pins;
			}

			return null;
		}

		// Get hot pins
		public async Task<List<Pin>> GetHotPins()
		{
			List<Pin> pins = new List<Pin>();
			var uri = new Uri(BASE_URL + "/api/pins?sort=likes&limit=10");
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				string content = await response.Content.ReadAsStringAsync();
				pins = JsonConvert.DeserializeObject<List<Pin>>(content);
				return pins;
			}
			return null;
		}

		// Create Pins
		public async Task<string> CreatePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			if (response.IsSuccessStatusCode)
			{
				var res = new JObject();
				res = JObject.Parse(await response.Content.ReadAsStringAsync());
				return (string)res["_id"];
			}
			return "";
		}

		// Delete Pin
		public async Task<bool> DeletePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id);
			var response = await httpClient.DeleteAsync(uri);
			return response.IsSuccessStatusCode;
		}

		// Update Pin
		public async Task<bool> UpdatePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await httpClient.PutAsync(uri, content);
			return response.IsSuccessStatusCode;
		}

		// Like Pin
		public async Task<bool> LikePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/likes");
			var response = await httpClient.PutAsync(uri, null);
			return response.IsSuccessStatusCode;
		}

		// Dislike Pin
		public async Task<bool> DislikePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/dislikes");
			var response = await httpClient.PutAsync(uri, null);
			return response.IsSuccessStatusCode;
		}

		// Add a comment 
		// Need to pass {text:<string>, linkedAccount:<string>}
		public async Task<bool> AddCommentToPin(Pin pin, string text, string linkedAccount)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/comments");
			JObject comment = new JObject();
			comment.Add("text", text);
			comment.Add("linkedAccount", linkedAccount);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(comment), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			return response.IsSuccessStatusCode;
		}

		// Delete a comment 
		// Need to pass the pin and account ID
		public async Task<bool> DeleteCommentFromPin(Pin pin, string accountId)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/comments/" + accountId);
			var response = await httpClient.DeleteAsync(uri);
			return response.IsSuccessStatusCode;
		}

        // Get Accounts
        public async Task<Account> GetAccount()
        {
            var uri = new Uri(BASE_URL + "/api/accounts/currentuser");
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject accountParsed = JObject.Parse(content);
                Account account = new Account
                {
                    Id = (string)accountParsed["_id"],
                    Username = (string)accountParsed["auth0Id"],
                    Seeds = (double)accountParsed["numSeeds"],
                    Pins = (int)accountParsed["numPins"]
                };

                return account;
            }

            return null;
        }

		// Create Account
		// Need to pass {username:<string>, token:<string>}
		public async Task<bool> CreateAccount(JObject account)
		{
			var uri = new Uri(BASE_URL + "/api/accounts");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(account.ToString(), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			return response.IsSuccessStatusCode;
		}

		// Change seed amount of account
		// Need to pass account ID and amount
		public async Task<bool> UpdateAccountSeeds(string accountId, double amount)
		{
			var uri = new Uri(BASE_URL + "/api/accounts/seeds/" + accountId + "/" + amount);
			var response = await httpClient.PutAsync(uri, null);
			return response.IsSuccessStatusCode;
		}


		// Getting an image from a url
		public async Task<byte[]> GetImageBitmapFromUrlAsync(string url)
		{
			using (var client = new HttpClient())
			{
				client.MaxResponseContentBufferSize = 256000;
				try
				{
					var result = await client.GetAsync(url);
					if (result.IsSuccessStatusCode)
					{
						return await result.Content.ReadAsByteArrayAsync();
					}
					else
					{
						Debug.WriteLine(result.StatusCode);
						return null;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					return null;
				}
			}

		}

		// Imgur post image
		public async Task<string> UploadImage(byte[] img)
		{
			var parms = new JObject();
			parms.Add("image", img);
			var uri = new Uri(IMGUR_BASE_URL + "/image");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", IMGUR_CLIENT_ID);
			var content = new StringContent(parms.ToString(), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			if (response.IsSuccessStatusCode)
			{
				var res = new JObject();
				res = JObject.Parse(await response.Content.ReadAsStringAsync());
				return (string)res["data"]["link"];
			}
			return "";
		}

    }
}
