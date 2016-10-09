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

namespace DayTomato.Services
{
    public class DayTomatoClient
    {
        HttpClient httpClient;

#if DEBUG
		private readonly string BASE_URL = "http://10.0.2.2:8080";
#else
		private readonly string BASE_URL = "http://fridayideas.herokuapp.com";
#endif

		private readonly string GOOGLE_API_KEY = "AIzaSyDU2aOZLIaBsZ4s62PQ1T88e9UL0QvLsoA";
		private readonly string GOOGLE_PLACES_BASE_URL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=";
		private readonly string GOOGLE_PLACES_PHOTO_BASE_URL = "https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference=";
		private readonly string GOOGLE_PLACES_DETAIL_BASE_URL = "https://maps.googleapis.com/maps/api/place/details/json?placeid=";

        public DayTomatoClient()
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
        }

		// Get Place
		public async Task<Place> GetPlace(double lat, double lng)
		{
			Place place = new Place();
			string result = "";
			var uri = new Uri(GOOGLE_PLACES_BASE_URL + lat + "," + lng + "&radius=100&rankby=distance&key=" + GOOGLE_API_KEY);
			var response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				result = await response.Content.ReadAsStringAsync();
				place = JsonConvert.DeserializeObject<Place>(result);
			}

			uri = new Uri(GOOGLE_PLACES_PHOTO_BASE_URL + place.PhotoReference + "&key=" + GOOGLE_API_KEY);
			response = await httpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				place.Image = await response.Content.ReadAsByteArrayAsync();
			}

			return place;
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

		// Create Pins
		public async Task<bool> CreatePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			return response.IsSuccessStatusCode;
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

		// Add a review 
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

		// Delete a review 
		// Need to pass the pin and account ID
		public async Task<bool> DeleteReviewFromPin(Pin pin, string accountId)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/comments/" + accountId);
			var response = await httpClient.DeleteAsync(uri);
			return response.IsSuccessStatusCode;
		}

        // Get Accounts
        public async Task<Account> GetAccount(string accountId)
        {
            Account account = new Account();
            var uri = new Uri(BASE_URL + "/api/accounts/" + accountId);
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                account = JsonConvert.DeserializeObject<Account>(content);
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
    }
}
