using System;
using System.Collections.Generic;
using System.Threading;
using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering.View;
using Com.Google.Maps.Android.Clustering;
using Android.Gms.Maps;
using Android.Content;
using Java.Lang;

namespace DayTomato.Droid
{
	public class ClusterRenderer : DefaultClusterRenderer
	{

		public ClusterRenderer(Context context, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
        {
            clusterManager.SetRenderer(this);
        }
        
        protected override void OnBeforeClusterItemRendered(Java.Lang.Object markerItem, MarkerOptions markerOptions)
        {
            var newMarkerItem = (ClusterPin)markerItem;
            if (newMarkerItem.getIcon() != null)
            {
                markerOptions.SetIcon(newMarkerItem.getIcon()); //Here you retrieve BitmapDescriptor from ClusterItem and set it as marker icon
            }
            markerOptions.Visible(true);
        }
    }
}
