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
        public BitmapDescriptor icon;

		public ClusterPin(double lat, double lng, string title, BitmapDescriptor icon)
		{
			Position = new LatLng(lat, lng);
            Title = title;
            this.icon = icon;

			// Ensure that each cluster pin has a unique Id
			Id = Interlocked.Increment(ref nextID);
		}

        public BitmapDescriptor getIcon()
        {
            return icon;
        }

    }
}
