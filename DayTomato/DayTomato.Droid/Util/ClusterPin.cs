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

		public ClusterPin(double lat, double lng)
		{
			Position = new LatLng(lat, lng);

			// Ensure that each cluster pin has a unique Id
			Id = Interlocked.Increment(ref nextID);
		}

	}
}
