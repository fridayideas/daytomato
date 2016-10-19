using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayTomato.Models
{
	public class Account
	{
		// NOOB = 0-10 seeds, EXPERIENCED = 10-200, seeds, PRO = 200-1000 seeds, ESTABLISHED = 1000-20000 seeds, TRUSTED = 20000-25000 seeds, GOD = 25000+ seeds
		public enum SeedLevels { NOOB, EXPERIENCED, PRO, ESTABLISHED, TRUSTED, GOD };

		// Images are not stored in server, rather, they will be saved locally on the phone
		public string Id { get; set; }
		public string Username { get; set; }
		public double Seeds { get; set; }
		public int Pins { get; set; }
		public Byte[] ProfilePicture { get; set; }
		public SeedLevels Privilege { get; set; }
	    public string UserJson { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }

		public SeedLevels GetAccountPrivileges()
		{
			if (Seeds < 10)
				return SeedLevels.NOOB;
			else if (Seeds < 200)
				return SeedLevels.EXPERIENCED;
			else if (Seeds < 1000)
				return SeedLevels.PRO;
			else if (Seeds < 20000)
				return SeedLevels.ESTABLISHED;
			else if (Seeds < 25000)
				return SeedLevels.TRUSTED;
			else
				return SeedLevels.GOD;
		}

        public class AccountConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Pin));
            }

            //Read REST API Json
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                Account account = new Account();

                account.Id = (string)jo["_id"];                      // Id of account
                account.Username = (string)jo["username"];           // Name of user
                account.Seeds = (double)jo["seeds"];                 // Number of seeds user owns
                account.Pins = (int)jo["pins"];                      // Number of pins user owns

                return account;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
