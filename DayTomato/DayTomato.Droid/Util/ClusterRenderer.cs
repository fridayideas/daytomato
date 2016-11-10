using System;
using System.Collections.Generic;
using System.Threading;
using Android.Gms.Maps.Model;
using Com.Google.Maps.Android.Clustering.View;
using Com.Google.Maps.Android.Clustering;
using Android.Gms.Maps;
using Android.Content;
using Java.Lang;
using Com.Google.Maps.Android.UI;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace DayTomato.Droid
{
    

	public class ClusterRenderer : DefaultClusterRenderer
	{
        private static Drawable TRANSPARENT_DRAWABLE = new ColorDrawable(Color.Transparent);
        IconGenerator mIconGenerator;
        ImageView mImageView;
        

        public ClusterRenderer(Context context, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
        {
            clusterManager.SetRenderer(this);

            mIconGenerator = new IconGenerator(context);
            mImageView = new ImageView(context);
            mIconGenerator.SetContentView(mImageView);
        }
        
        protected override void OnBeforeClusterItemRendered(Java.Lang.Object markerItem, MarkerOptions markerOptions)
        {
            var newMarkerItem = (ClusterPin)markerItem;
            if (newMarkerItem.getIcon() != 0)
            {
                mImageView.SetImageResource(newMarkerItem.getIcon());
                mIconGenerator.SetBackground(TRANSPARENT_DRAWABLE);
                Bitmap icon = mIconGenerator.MakeIcon();
                Bitmap resized = Bitmap.CreateScaledBitmap(icon, 110, 166, false);
                markerOptions.SetIcon(BitmapDescriptorFactory.FromBitmap(resized)); //Here you retrieve BitmapDescriptor from ClusterItem and set it as marker icon
            }
            markerOptions.Visible(true);
        }
    }
}
