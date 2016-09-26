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
        private readonly string BASE_URL = "http://fridayideas-db.herokuapp.com";
        
        public DayTomatoClient()
        {
            httpClient = new HttpClient();
            httpClient.MaxResponseContentBufferSize = 256000;
        }

        // Get Pins
        public async Task<string> GetPins()
        {
            List<Pin> pins = new List<Pin>();
            var uri = new Uri(BASE_URL + "/api/pins");
            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
                //pins = JsonConvert.DeserializeObject<List<Pin>>(content);
            }

            return "";
        }
    }
}
