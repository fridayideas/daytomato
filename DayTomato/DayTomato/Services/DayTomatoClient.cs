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
