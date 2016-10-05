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
        private readonly string BASE_URL = "http://fridayideas.herokuapp.com";
        
        public DayTomatoClient()
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
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
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Delete Pin
		public async Task<bool> DeletePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id);
			var response = await httpClient.DeleteAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Update Pin
		public async Task<bool> UpdatePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await httpClient.PutAsync(uri, content);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Like Pin
		public async Task<bool> LikePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/likes");
			var response = await httpClient.PutAsync(uri, null);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Dislike Pin
		public async Task<bool> DislikePin(Pin pin)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/dislikes");
			var response = await httpClient.PutAsync(uri, null);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Add a review 
		// Need to pass {text:<string>, linkedAccount:<string>}
		public async Task<bool> AddReviewToPin(Pin pin, JObject review)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/review");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var content = new StringContent(review.ToString(), Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(uri, content);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Delete a review 
		// Need to pass the pin and account ID
		public async Task<bool> DeleteReviewFromPin(Pin pin, string accountId)
		{
			var uri = new Uri(BASE_URL + "/api/pins/" + pin.Id + "/reviews/" + accountId);
			var response = await httpClient.DeleteAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
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
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		// Change seed amount of account
		// Need to pass account ID and amount
		public async Task<bool> UpdateAccountSeeds(string accountId, double amount)
		{
			var uri = new Uri(BASE_URL + "/api/accounts/seeds/" + accountId + "/" + amount);
			var response = await httpClient.PutAsync(uri, null);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}
    }
}
