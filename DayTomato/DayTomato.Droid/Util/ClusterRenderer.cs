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
        private Drawable TRANSPARENT_DRAWABLE = new ColorDrawable(Color.Transparent);
        IconGenerator _iconGenerator;
        ImageView _imageView;
        

        public ClusterRenderer(Context context, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
        {
            clusterManager.SetRenderer(this);

            _iconGenerator = new IconGenerator(context);
            _imageView = new ImageView(context);
            _iconGenerator.SetContentView(_imageView);
        }
        
        protected override void OnBeforeClusterItemRendered(Java.Lang.Object markerItem, MarkerOptions markerOptions)
        {
            var newMarkerItem = (ClusterPin)markerItem;
            if (newMarkerItem.iconResId != 0)
            {
                _imageView.SetImageResource(newMarkerItem.iconResId);
                _iconGenerator.SetBackground(TRANSPARENT_DRAWABLE);
                Bitmap icon = _iconGenerator.MakeIcon();
                Bitmap resized = Bitmap.CreateScaledBitmap(icon, (int)((double)icon.Width/1.5), (int)((double)icon.Height/1.5), false);
                markerOptions.SetIcon(BitmapDescriptorFactory.FromBitmap(resized)); //Here you retrieve BitmapDescriptor from ClusterItem and set it as marker icon
            }
            markerOptions.Visible(true);
        }
    }
}
