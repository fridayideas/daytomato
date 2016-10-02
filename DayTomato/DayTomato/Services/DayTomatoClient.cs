using DayTomato.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        // Get Accounts
        public async Task<Account> GetAccount(string accountId)
        {
            Account account = new Account();
            var uri = new Uri(BASE_URL + "/api/account/" + accountId);
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                account = JsonConvert.DeserializeObject<Account>(content);
                return account;
            }

            return null;
        }
    }
}
