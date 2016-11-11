using System;
using System.Collections.Generic;
using System.Threading;
using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering;

namespace DayTomato.Droid
{
	public class ClusterPin : Java.Lang.Object, IClusterItem
	{
		private static long nextID;

		public long Id { get; private set; }
		public string Title { get; set; }
		public LatLng Position { get; set; }
        public int iconResId { get; set; }

		public ClusterPin(double lat, double lng, string title, int icon)
		{
			Position = new LatLng(lat, lng);
            Title = title;
            this.iconResId = icon;

			// Ensure that each cluster pin has a unique Id
			Id = Interlocked.Increment(ref nextID);
		}

        public ClusterPin(double lat, double lng, string title)
        {
            Position = new LatLng(lat, lng);
            Title = title;

            // Ensure that each cluster pin has a unique Id
            Id = Interlocked.Increment(ref nextID);
        }

    }
}
