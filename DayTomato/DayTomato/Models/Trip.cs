using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayTomato.Models
{
    public class Trip
    {
        public Trip(List<Pin> pins)
        {
            Pins = pins;
            NumberOfActivities = pins.Count;
        }

        public List<Pin> Pins { get; set; }
        public long Duration { get; set; }
        public int NumberOfActivities { get; set; }
    }
}
