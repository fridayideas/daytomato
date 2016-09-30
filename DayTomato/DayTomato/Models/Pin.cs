using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DayTomato.Models
{
    /*
    {
        "_id": <ObjectId>
        "rating": <string>,
        "description": <string>, 
        "likes" : <int>,
        "duration": <long>,
        "coordinate": {
            "latitude": <double>,
            "longitude": <double>
        },
        "linkedAccount": <ObjectId>, 
        "reviews": <List>
    }
    */

    [JsonConverter(typeof(PinConverter))]
    public class Pin
    {
        private string _id;
        private int _type;
        private string _name;
        private string _rating;
        private string _description;
        private int _likes;
        private long _duration;
        private double _latitude;
        private double _longitude;
        private long _linkedAccount;
        private List<Review> _reviews;
        private DateTime _createDate;

        public string Id { get { return _id; } set { _id = value; } }

        // Type of pin depends on the amount of seeds the user has
        // 0-10 seeds normal user; 10-50 seeds, ...
        public int Type { get { return _type; } set { _type = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string Rating { get { return _rating; } set { _rating = value; } } 
        public string Description { get { return _description; } set { _description = value; } }
        public int Likes { get { return _likes; } set { _likes = value; } }
        public long Duration { get { return _duration; } set { _duration = value; } }
        public double Latitude { get { return _latitude; } set { _latitude = value; } }
        public double Longitude { get { return _longitude; } set { _longitude = value; } }
        public long LinkedAccount { get { return _linkedAccount; } set { _linkedAccount = value; } }
        public List<Review> Reviews { get { return _reviews; } set { _reviews = value; } }
        public DateTime CreateDate { get { return _createDate; } set { _createDate = value; } }
       
    }

    public class PinConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Pin));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Pin pin = new Pin();

            pin.Id = (string)jo["_id"];                             // Id of pin
            pin.Type = (int)jo["pinType"];                          // Type of pin
            pin.Name = (string)jo["pinName"];                       // Name of pin
            pin.Rating = (string)jo["rating"];                      // Rating of pin
            pin.Description = (string)jo["description"];            // Description of pin
            pin.Likes = (int)jo["likes"];                           // Pin likes
            pin.Duration = (long)jo["duration"];                    // Pin life duration
            pin.Latitude = (double)jo["coordinate"]["latitude"];    // Pin latitude
            pin.Longitude = (double)jo["coordinate"]["longitude"];  // Pin longitude
            pin.LinkedAccount = (long)jo["linkedAccount"];          // Pin linked account
            pin.CreateDate = (DateTime)jo["createDate"];            // Pin create date
   
            /* Rewviews
             * Reviews are in this format:
             * [{"linkedAccount":111,"text":"LOOOOL","createDate":"2016-09-30T02:44:20.637Z"}]
             */
            JArray ja = (JArray)jo["reviews"];
            for(int i = 0; i < ja.Count; ++i)
            {
                long reviewAccount = (long)ja[i]["linkedAccount"];
                string reviewText = (string)ja[i]["text"];
                DateTime reviewCreateDate = (DateTime)ja[i]["createDate"];
                pin.Reviews = new List<Review>();
                pin.Reviews.Add(new Review(reviewAccount, reviewText, reviewCreateDate));
            }

            return pin;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class Review
    {
        private long _linkedAccount;
        private string _text;
        private DateTime _createDate;
        public Review(long account, string text, DateTime date)
        {
            _linkedAccount = account;
            _text = text;
            _createDate = date;
        }
    }
}
