using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayTomato.Models
{
    public enum Rating { ZERO_STAR, ONE_STAR, TWO_STAR, THREE_STAR, FOUR_STAR, FIVE_STAR };

    // FREE = $0; LOW = $0 - $10; LOWMEDIUM = $10 - $20; MEDIUM = $20 - $30; MEDIUMHIGH = $30 - $50; HIGH = $50+
    public enum Cost { FREE, LOW, LOWMEDIUM, MEDIUM, MEDIUMHIGH, HIGH };
    public class Pin
    {
        public Rating ActivityRating { get; set; }
        public Cost ActivityCost { get; set; }
        public Coordinate ActivityCoordinate { get; set; }
        public string ActivityDescription { get; set; }
        public int PinLikes { get; set; }
        public long Duration { get; set; }
        public long AccountID { get; set; }
        public List<string> Reviews { get; set; }
    }

    public class Coordinate
    {
        public Coordinate(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
