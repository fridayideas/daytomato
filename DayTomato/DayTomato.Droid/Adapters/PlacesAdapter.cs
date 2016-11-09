
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DayTomato.Droid
{
	public class PlacesAdapter : RecyclerView.Adapter
	{
		private static readonly string TAG = "PLACES_ADAPTER";
		private List<Feed> _feed;
		private const int FEED_NOTIFICATION = 0;
		private const int FEED_PIN = 1;
		private Activity _context;

		public PlacesAdapter(List<Feed> feed, Activity context)
		{
			_feed = feed;
			_context = context;
		}

		public override int ItemCount
		{
			get { return _feed.Count; }
		}

		public override int GetItemViewType(int position)
		{
			return _feed[position].Type;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			RecyclerView.ViewHolder viewHolder = null;
			LayoutInflater inflater = LayoutInflater.From(parent.Context);

			switch (viewType)
			{
				case FEED_NOTIFICATION:
					View notification = inflater.
                        	Inflate(Resource.Layout.home_feed_notification_view_holder, parent, false);
					viewHolder = new HomeFeedNotificationViewHolder(notification);
					break;
				case FEED_PIN:
					View pin = inflater.
							Inflate(Resource.Layout.home_feed_pins_view_holder, parent, false);
					viewHolder = new HomeFeedPinViewHolder(pin);
					break;
			}

			return viewHolder;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			switch (holder.ItemViewType)
			{
				case FEED_NOTIFICATION:
					var notification = holder as HomeFeedNotificationViewHolder;
					ConfigureNotification(notification, position);
					break;
				case FEED_PIN:
					var pin = holder as HomeFeedPinViewHolder;
					ConfigurePin(pin, position);
					break;
			}
		}

		public void ConfigureNotification(HomeFeedNotificationViewHolder view, int position)
		{
			view.Notification.Text = _feed[position].Notification;
		} 

		public async void ConfigurePin(HomeFeedPinViewHolder view, int position)
		{
			view.Name.Text = _feed[position].Pin.Name;

			// Pin imageURL 
			try
			{
				var imageUrl = _feed[position].Pin.ImageURL;
				if (!imageUrl.Equals("none") && !imageUrl.Equals("") && imageUrl != null)
				{
					var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
					var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
					view.Image.SetImageBitmap(imageBitmap);
				}
			}
			catch (Exception ex)
			{
				Log.Debug(TAG, ex.Message);
			}
			view.Likes.Text = _feed[position].Pin.Likes + " likes";
			view.Cost.Text = "$" + _feed[position].Pin.Cost;
			view.Review.Text = _feed[position].Pin.Review;

			// Open google maps for directions to the hot place
			view.Directions.Click += (sender, e) => 
			{
				string lat = Convert.ToString(_feed[position].Pin.Coordinate.latitude);
				string lng = Convert.ToString(_feed[position].Pin.Coordinate.longitude);
				var geoUri = Android.Net.Uri.Parse("geo:0,0?q=" + lat + "," +lng + "(" + _feed[position].Pin.Name + ")");
				var mapIntent = new Intent(Intent.ActionView, geoUri);
				_context.StartActivity(mapIntent);
			};
		}
	}

	public class HomeFeedPinViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView Name { get; private set; }
		public ImageView Image { get; private set; }
		public TextView Likes { get; private set; }
		public TextView Cost { get; private set; }
		public TextView Review { get; private set; }
		public ImageView Directions { get; private set; }

		public HomeFeedPinViewHolder(View itemView) : base(itemView)
		{
			Name = itemView.FindViewById<TextView>(Resource.Id.home_feed_pin_name);
			Image = itemView.FindViewById<ImageView>(Resource.Id.home_feed_pin_image);
			Likes = itemView.FindViewById<TextView>(Resource.Id.home_feed_pin_likes);
			Cost = itemView.FindViewById<TextView>(Resource.Id.home_feed_pin_cost);
			Review = itemView.FindViewById<TextView>(Resource.Id.home_feed_pin_review);
			Directions = itemView.FindViewById<ImageView>(Resource.Id.home_feed_pin_directions);
		}

		public void SetClickListener(Action<int> listener)
		{
			_listener = listener;
			ItemView.Click += HandleClick;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (ItemView != null)
			{
				ItemView.Click -= HandleClick;
			}
			_listener = null;
		}

		void HandleClick(object sender, EventArgs e)
		{
			_listener?.Invoke(base.AdapterPosition);
		}
	}

	public class HomeFeedNotificationViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView Notification { get; private set; }

		public HomeFeedNotificationViewHolder(View itemView) : base(itemView)
		{
			Notification = itemView.FindViewById<TextView>(Resource.Id.home_feed_notification);
		}

		public void SetClickListener(Action<int> listener)
		{
			_listener = listener;
			ItemView.Click += HandleClick;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (ItemView != null)
			{
				ItemView.Click -= HandleClick;
			}
			_listener = null;
		}

		void HandleClick(object sender, EventArgs e)
		{
			_listener?.Invoke(base.AdapterPosition);
		}
	}
}
