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
        // Images are not stored in server, rather, they will be saved locally on the phone
        private string _id;
        private string _username;
        private double _seeds;
        private int _pins;
        private Byte[] _profilePicture;

        public string Id { get { return _id; } set { _id = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public double Seeds { get { return _seeds; } set { _seeds = value; } }
        public int Pins { get { return _pins; } set { _pins = value; } }
        public Byte[] ProfilePicture { get { return _profilePicture; } set { _profilePicture = value; } }

        public class AccountConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Pin));
            }

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
