using DayTomato.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://fridayideas.herokuapp.com/api/";

        public DayTomatoClient(string idToken)
		{
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                MaxResponseContentBufferSize = 256000
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		// Get All Trips
		public async Task<List<Trip>> GetTrips()
		{
			var response = await _httpClient.GetAsync("trips");
		    response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Trip>>(content);
        }

        public async Task<string> CreateTrip(CreateTrip trip)
        {
            var content = new StringContent(JsonConvert.SerializeObject(trip), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("trips", content);
            response.EnsureSuccessStatusCode();

            var res = JObject.Parse(await response.Content.ReadAsStringAsync());
            return (string)res["_id"];
        }

        public async Task<List<Trip>> GetMyTrips(Account account)
        {
            var response = await _httpClient.GetAsync($"accounts/{account.Id}/trips");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(); 
            return JsonConvert.DeserializeObject<List<Trip>>(content);
        }
        public async Task<bool> AddToMyTrips(string tripId, Account account)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new 
            {
                myTrips = tripId
            }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"accounts/{account.Id}/trips", content);

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> RemoveFromMyTrips(string tripId, Account account)
        {
            var response = await _httpClient.DeleteAsync($"accounts/{account.Id}/trips/{tripId}");
            return response.IsSuccessStatusCode;
        }

		public async Task<bool> DeleteTrip(string tripId)
		{
			var response = await _httpClient.DeleteAsync("trips/" + tripId);
			return response.IsSuccessStatusCode;
		}

        private async Task<bool> CastVoteOn(string thing, string thingId, Account account, int dir)
        {
			var content = new StringContent(JsonConvert.SerializeObject(new { dir }),
                Encoding.UTF8, "application/json");
			var response = await _httpClient.PutAsync($"{thing}/{thingId}/votes/{account.Id}", content);
			return response.IsSuccessStatusCode;
        }

		// Like Trip
        public Task<bool> LikeTrip(string tripId, Account account) => CastVoteOn("trips", tripId, account, 1);

		// Dislike Trip
        public Task<bool> DislikeTrip(string tripId, Account account) => CastVoteOn("trips", tripId, account, -1);

		// Remove Vote from Trip
        public Task<bool> RemoveVoteTrip(string tripId, Account account) => CastVoteOn("trips", tripId, account, 0);

        // Get Pins
        public async Task<List<Pin>> GetPins(string sortBy = "createDate", int sortDir = 1, int limit = 0)
		{
		    var response = await _httpClient.GetAsync($"pins?sort={sortBy}&sortdir={sortDir}&limit={limit}");
		    response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Pin>>(content);
        }

        // Get hot pins
        public Task<List<Pin>> GetHotPins() => GetPins("likes", -1, 10);

        // Get a Pin
        public async Task<Pin> GetPin(string id)
        {
            var response = await _httpClient.GetAsync("pins/" + id);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Pin>(content);
        }

        // Get Pins In Area
        public async Task<List<Pin>> __GetPins(double latTopLeft, double lngTopLeft, double latBotRight, double lngBotRight)
		{
			List<Pin> pins = new List<Pin>();
			var uri = new Uri(BaseUrl + "pins?searcharea=" + Convert.ToString(latTopLeft) + ","
			                  									 + Convert.ToString(lngTopLeft) + ","
			                 									 + Convert.ToString(latBotRight) + ","
			                 									 + Convert.ToString(lngBotRight));
			var response = await _httpClient.GetAsync(uri);
		    response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Pin>>(content);
        }

        // Create Pins
        public async Task<string> CreatePin(Pin pin)
		{
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync("pins", content);
		    response.EnsureSuccessStatusCode();

            var res = JObject.Parse(await response.Content.ReadAsStringAsync());
            return (string)res["_id"];
        }

        // Delete Pin
        public async Task<bool> DeletePin(Pin pin)
		{
			var response = await _httpClient.DeleteAsync("pins/" + pin.Id);
			return response.IsSuccessStatusCode;
		}

		// Update Pin
		public async Task<bool> UpdatePin(Pin pin)
		{
			var content = new StringContent(JsonConvert.SerializeObject(pin), Encoding.UTF8, "application/json");
			var response = await _httpClient.PutAsync("pins/" + pin.Id, content);
			return response.IsSuccessStatusCode;
		}

		// Like Pin
        public Task<bool> LikePin(string pinId, Account account) => CastVoteOn("pins", pinId, account, 1);

		// Dislike Pin
        public Task<bool> DislikePin(string pinId, Account account) => CastVoteOn("pins", pinId, account, -1);

		// Remove Votes Pin
		public Task<bool> RemoveVotePin(string pinId, Account account) => CastVoteOn("pins", pinId, account, 0);

		// Add a comment 
		// Need to pass {text:<string>, linkedAccount:<string>}
		public async Task<bool> AddCommentToPin(Pin pin, string text, string linkedAccount)
		{
			var content = new StringContent(JsonConvert.SerializeObject(new { text, linkedAccount }),
                Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync($"pins/{pin.Id}/comments", content);
			return response.IsSuccessStatusCode;
		}

		// Delete a comment 
		// Need to pass the pin and account ID
		public async Task<bool> DeleteCommentFromPin(Pin pin, string accountId)
		{
			var response = await _httpClient.DeleteAsync($"pins/{pin.Id}/comments/{accountId}");
			return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Generates an Account object by calling a GET request to 
        /// the server. The server uses the IdToken (included in the httpClient)
        /// to return an account object of the form: 
        /// * {
        /// *   "_id": string "mongo ID";,
        /// *   "auth0Id": string "auth0Id",
        /// *   "seeds": double seeds,
        /// *   "pins": int pins
        /// * }
        /// </summary>
        /// <returns></returns>
        public async Task<Account> GetAccount()
        {
            var response = await _httpClient.GetAsync("accounts/currentuser");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var accountParsed = JObject.Parse(content);
            return new Account
            {
                Id = (string)accountParsed["_id"],
                Username = (string)accountParsed["auth0Id"], // TODO: This is a temporary placeholder for username (should use a real username once /accounts/currentuser route is updated)
                Seeds = (double)accountParsed["numSeeds"],
                Pins = (int)accountParsed["numPins"]
            };
        }

        // Create Account
        // Need to pass {username:<string>, token:<string>}
        public async Task<bool> CreateAccount(JObject account)
		{
			var content = new StringContent(account.ToString(), Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync("accounts", content);
			return response.IsSuccessStatusCode;
		}

		// Change seed amount of account
		// Need to pass account ID and amount
        // TODO do we need this?
		public async Task<bool> UpdateAccountSeeds(string accountId, double amount)
		{
			var uri = new Uri(BaseUrl + "/api/accounts/seeds/" + accountId + "/" + amount);
			var response = await _httpClient.PutAsync(uri, null);
			return response.IsSuccessStatusCode;
		}

		// Change pin amount of account
		// Need to pass account ID and amount
        // TODO do we need this?
		public async Task<bool> UpdateAccountPins(string accountId, int amount)
		{
			var uri = new Uri(BaseUrl + "/api/accounts/pins/" + accountId + "/" + amount);
			var response = await _httpClient.PutAsync(uri, null);
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
    }
}
