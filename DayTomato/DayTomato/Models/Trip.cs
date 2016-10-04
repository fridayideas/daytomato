using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayTomato.Models
{
    public class Trip
    {
        public Trip(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public List<Pin> Pins { get; set; }
        public long Duration { get; set; }
        public int NumberOfActivities { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
    }
}
