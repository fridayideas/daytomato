using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DayTomato
{
	public class ImgurClient
	{
		HttpClient httpClient;
		private readonly string IMGUR_BASE_URL = "https://api.imgur.com/3";
		private readonly string IMGUR_CLIENT_ID = "1f30123ee30a53b";
		private readonly string IMGUR_CLIENT_SECRET = "979732009beba54d18e67f6dc9f8c3fa79082d16";

		public ImgurClient()
		{
			httpClient = new HttpClient();
			httpClient.MaxResponseContentBufferSize = 256000;
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", IMGUR_CLIENT_ID);
		}

		// Imgur post image
		public async Task<string> UploadImage(byte[] img)
		{
			var parms = new JObject();
			parms.Add("image", img);
			var uri = new Uri(IMGUR_BASE_URL + "/image");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
