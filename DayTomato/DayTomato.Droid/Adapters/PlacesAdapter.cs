
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	public class PlacesAdapter : RecyclerView.Adapter
	{
		private static readonly string TAG = "PLACES_ADAPTER";
		private List<Pin> _pins;
		private Activity _context;

		public PlacesAdapter(List<Pin> pins, Activity context)
		{
			_pins = pins;
			_context = context;
		}

		public override int ItemCount
		{
			get { return _pins.Count; }
		}

		public override int GetItemViewType(int position)
		{
			return _pins[position].Type;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			RecyclerView.ViewHolder viewHolder = null;
			LayoutInflater inflater = LayoutInflater.From(parent.Context);

			View pin = inflater.Inflate(Resource.Layout.places_view_holder, parent, false);
			viewHolder = new PlaceViewHolder(pin);

			return viewHolder;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var pin = holder as PlaceViewHolder;
			ConfigurePin(pin, position);
		}

		public async void ConfigurePin(PlaceViewHolder view, int position)
		{
			view.Name.Text = _pins[position].Name;

			// Pin imageURL 
			var imageUrl = _pins[position].ImageURL;
			if (!imageUrl.Equals("none") && !imageUrl.Equals("") && imageUrl != null)
			{
				try
				{
					var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
					var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
					view.Image.SetImageBitmap(imageBitmap);
				}
				catch (Exception ex)
				{
					Log.Error(TAG, ex.Message);
				}
			}

			SetCost(view, _pins[position].Cost);
			view.RatingText.Text = _pins[position].Rating.ToString();
			view.Rating.Rating = _pins[position].Rating;
			view.Likes.Text = _pins[position].Likes + " likes";
			view.Review.Text = _pins[position].Review;

			// Open google maps for directions to the hot place
			view.Directions.Click += (sender, e) => 
			{
				string lat = Convert.ToString(_pins[position].Coordinate.latitude);
				string lng = Convert.ToString(_pins[position].Coordinate.longitude);
				var geoUri = Android.Net.Uri.Parse("geo:0,0?q=" + lat + "," +lng + "(" + _pins[position].Name + ")");
				var mapIntent = new Intent(Intent.ActionView, geoUri);
				_context.StartActivity(mapIntent);
			};
		}

		private void SetCost(PlaceViewHolder vh, double cost)
		{
			if (cost > 0)
			{
				vh.Cost.Text = "$" + cost;
			}
			else
			{
				vh.Cost.Text = "FREE";
			}
		}
	}

	public class PlaceViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView Name { get; private set; }
		public ImageView Image { get; private set; }
		public TextView Likes { get; private set; }
		public TextView Cost { get; private set; }
		public TextView RatingText { get; private set; }
		public RatingBar Rating { get; private set; }
		public TextView Review { get; private set; }
		public ImageView Directions { get; private set; }

		public PlaceViewHolder(View itemView) : base(itemView)
		{
			Name = itemView.FindViewById<TextView>(Resource.Id.place_name);
			Image = itemView.FindViewById<ImageView>(Resource.Id.place_image);
			Likes = itemView.FindViewById<TextView>(Resource.Id.place_likes);
			Cost = itemView.FindViewById<TextView>(Resource.Id.place_cost);
			RatingText = itemView.FindViewById<TextView>(Resource.Id.place_rating_text);
			Rating = itemView.FindViewById<RatingBar>(Resource.Id.place_rating);
			Review = itemView.FindViewById<TextView>(Resource.Id.place_review);
			Directions = itemView.FindViewById<ImageView>(Resource.Id.place_directions);
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
			_listener?.Invoke(AdapterPosition);
		}
	}
}
