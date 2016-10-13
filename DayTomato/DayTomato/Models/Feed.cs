
using DayTomato.Models;

namespace DayTomato
{
	public class Feed
	{
		public static readonly int FEED_NOTIFICATION = 0;
		public static readonly int FEED_PIN = 1;
		public int Type { get; set; }
		public string Notification { get; set; }
		public Pin Pin { get; set; } 
	}
}
