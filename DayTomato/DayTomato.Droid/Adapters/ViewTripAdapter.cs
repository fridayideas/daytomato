using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid.Adapters
{
	public class ViewTripAdapter : RecyclerView.Adapter
	{
		// Create an Event so that our our clients can act when a user clicks
		// on each individual item.
		public event EventHandler<int> HandleClick;

		private readonly string Tag = "VIEW_TRIP_ADAPTER";
		private List<Trip> _trips;
		private List<bool> _tripLiked;
		private List<bool> _tripDisliked;
		private readonly Activity _context;
		private readonly Account _account;

		public ViewTripAdapter(List<Trip> trips, Activity context)
		{
			_trips = trips;
			_tripLiked = new List<bool>(new bool[_trips.Count]);
			_tripDisliked = new List<bool>(new bool[_trips.Count]);
			_context = context;
			_account = MainActivity.GetAccount();
		}

		public override int ItemCount => _trips.Count;

	    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			var itemView = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.trip_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			return new TripSuggestionViewHolder(itemView, OnClick);
		}

	    private void RefreshComments(LinearLayout ll, CommentsAdapter ca)
		{
			ll.RemoveAllViews();
			for (var i = 0; i < ca.Count; i++)
			{
				var v = ca.GetView(i, null, ll);
				ll.AddView(v);
			}
			ca.NotifyDataSetChanged();
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
            var vh = (TripSuggestionViewHolder)holder;
		    var trip = _trips[position];
		    vh.Name.Text = trip.Name;
			vh.Type.Text = trip.Type;
		    vh.Pins.Text = trip.Pins
                .Aggregate("", (text, pin) => text + pin.Name + "\n");
			vh.CreateDate.Text = "created " + trip.CreateDate.ToLongDateString();
			vh.Account.Text = trip.Username;
			vh.Likes.Text = trip.Likes.ToString();
			vh.Description.Text = trip.Description;
		    vh.Cost.Text = trip.Cost > 0.0 ? $"${trip.Cost}" : "FREE";
			SetImage(vh, trip.Pins);
			vh.HideComments = true;
			try
			{
				if (trip.LikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					_tripLiked[position] = true;
					_tripDisliked[position] = false;
				}
				else if (trip.DislikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					_tripLiked[position] = false;
					_tripDisliked[position] = true;
				}
			}
			catch (Exception ex)
			{
				Log.Debug(Tag, ex.Message);
			}

			if (trip.LinkedAccount == MainActivity.GetAccount().Id || _context.ComponentName.ShortClassName.Contains("MainActivity"))
			{
				vh.Menu.Visibility = ViewStates.Visible;
				vh.Menu.Click += (sender, e) =>
			   	{
				   	var menu = new Android.Support.V7.Widget.PopupMenu(_context, vh.Menu, (int)GravityFlags.End);
				   	menu.Inflate(Resource.Menu.view_trip_popup_menu);

				   	menu.MenuItemClick += (s1, arg1) =>
				   	{
					   	var command = arg1.Item.TitleFormatted.ToString();
					   	if (command.Equals("Delete"))
					   	{
							try
							{
							   ((DeleteTripListener)_context).OnDeleteTrip(trip);
							}
							catch (Exception ex)
							{
							   Log.Debug(Tag, ex.Message);
							}
							Toast.MakeText(_context, "Removed " + trip.Name, ToastLength.Long).Show();
						    NotifyItemRemoved(position);
						    NotifyDataSetChanged();
					    }
				    };
					menu.Show();
				};
			}

			// Initializing listview
			vh.CommentsAdapter = new CommentsAdapter(_context, trip.Comments);
			// Make sure we can see the comments
			if (!vh.HideComments)
			{
				RefreshComments(vh.CommentsListView, vh.CommentsAdapter);
			}

			// When clicking add comment, show an edit text
			vh.AddComment.Click += (sender, e) =>
			{
				vh.AddCommentInput.Visibility = ViewStates.Visible;
				vh.AddCommentButton.Visibility = ViewStates.Visible;
			};
			// When the user presses add, add the new comment
			vh.AddCommentButton.Click += (sender, e) =>
			{
				vh.AddCommentInput.Visibility = ViewStates.Gone;
				vh.AddCommentButton.Visibility = ViewStates.Gone;

				var account = MainActivity.GetAccount();
				if (trip.Comments.Count > 0 && trip.Comments.Last().Text == vh.AddCommentInput.Text)
					return;
				trip.Comments.Add(new Comment(account.Id, account.Username, vh.AddCommentInput.Text, DateTime.Today));
				// await MainActivity.dayTomatoClient.AddCommentToTrip(_suggestions[position], vh.AddCommentInput.Text, account.Id);
				RefreshComments(vh.CommentsListView, vh.CommentsAdapter);
				vh.HideComments = !vh.HideComments;
				vh.CommentsListView.Visibility = ViewStates.Visible;
				vh.ShowComments.Text = "hide comments";
				vh.AddCommentInput.Text = "";
			};
			vh.ShowComments.Click += (sender, e) =>
			{
				vh.HideComments = !vh.HideComments;
				vh.AddComment.Visibility = ViewStates.Visible;
				if (vh.HideComments)
				{ 
					
					vh.CommentsListView.RemoveAllViews();
					vh.CommentsListView.Visibility = ViewStates.Gone;
					vh.AddComment.Visibility = ViewStates.Gone;
					vh.AddCommentInput.Visibility = ViewStates.Gone;
					vh.AddCommentButton.Visibility = ViewStates.Gone;
					vh.ShowComments.Text = "show comments";
					vh.CommentsAdapter.NotifyDataSetChanged();

				}
				else
				{
					vh.AddComment.Visibility = ViewStates.Visible;
					vh.CommentsListView.Visibility = ViewStates.Visible;
					vh.ShowComments.Text = "hide comments";
					RefreshComments(vh.CommentsListView, vh.CommentsAdapter);
				}
			};
			vh.UpButton.Click += async (sender, e) =>
			{
				// If the like and dislike button was not pressed, then its fresh
				if (!_tripLiked[position] && !_tripDisliked[position])
				{
					_tripLiked[position] = true;
					_tripDisliked[position] = false;
					trip.Likes++;
					vh.Likes.Text = trip.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (!trip.LikedBy.Contains(_account.Id) && !trip.DislikedBy.Contains(_account.Id))
					{
						trip.LikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.LikeTrip(trip.Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_tripDisliked[position])
				{
					_tripLiked[position] = false;
					_tripDisliked[position] = false;
					trip.Likes++;
					vh.Likes.Text = trip.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (trip.DislikedBy.Remove(_account.Id))
					{
						await MainActivity.dayTomatoClient.RemoveVoteTrip(trip.Id, _account);
					}
				}
			};
			vh.DownButton.Click += async (sender, e) =>
			{
				// If the like and dislike button was not pressed, then its fresh
				if (!_tripLiked[position] && !_tripDisliked[position])
				{
					_tripLiked[position] = false;
					_tripDisliked[position] = true;
					trip.Likes--;
					vh.Likes.Text = trip.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					if (!trip.LikedBy.Contains(_account.Id) && !trip.DislikedBy.Contains(_account.Id))
					{
						trip.DislikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.DislikeTrip(trip.Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_tripLiked[position])
				{
					_tripLiked[position] = false;
					_tripDisliked[position] = false;
					trip.Likes--;
					vh.Likes.Text = trip.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (trip.LikedBy.Remove(_account.Id))
					{
						await MainActivity.dayTomatoClient.RemoveVoteTrip(trip.Id, _account);
					}
				}
			};
		}

	    private async void SetImage(TripSuggestionViewHolder vh, IEnumerable<Pin> pins)
	    {
            var imageUrls = pins.Select(p => p.ImageURL)
                .Where(url => !string.IsNullOrEmpty(url) && url != "none");
	        var bitmaps = await Task.WhenAll(imageUrls.Select(async imageUrl =>
	        {
	            try
	            {
	                var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
	                var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
	                return imageBitmap;
	            }
	            catch (Exception ex)
	            {
	                Log.Error(Tag, ex.Message);
	                return null;
	            }
	        }).Where(bmp => bmp != null));

			try
			{
				var bitmap = PictureUtil.StitchImages(bitmaps);
				vh.StitchedImages.SetImageBitmap(bitmap);
			}
			catch (Exception ex)
			{
				Log.Error(Tag, ex.Message);
				vh.StitchedImages.SetImageBitmap(null);
			}
		}

		// This will fire any event handlers that are registered with our ItemClick event.
		private void OnClick(int position)
		{
            HandleClick?.Invoke(this, position);
        }
	}

	public class TripSuggestionViewHolder : RecyclerView.ViewHolder
	{
		public TextView Name { get; }
		public TextView Type { get; }
		public TextView Pins { get; }
		public TextView CreateDate { get; }
		public TextView Account { get; }
		public ImageView UpButton { get; }
		public TextView Likes { get; }
		public ImageView DownButton { get; }
		public TextView Description { get; }
		public ImageView StitchedImages { get; }
		public TextView Cost { get; }
		public TextView AddComment { get; }
		public EditText AddCommentInput { get; }
		public Button AddCommentButton { get; }
		public TextView ShowComments { get; }
		public bool HideComments { get; set; }
		public LinearLayout CommentsListView { get; }
		public CommentsAdapter CommentsAdapter { get; set; }
		public ImageView Menu { get; }

		public TripSuggestionViewHolder(View itemView, Action<int> listener) : base(itemView)
		{
			Name = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_name);
			Type = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_type);
			Pins = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_pins);
			CreateDate = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_create_date);
			Account = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_account);
			UpButton = itemView.FindViewById<ImageView>(Resource.Id.trip_suggestion_up_button);
			Likes = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_likes);
			DownButton = itemView.FindViewById<ImageView>(Resource.Id.trip_suggestion_down_button);
			Description = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_description);
			StitchedImages = itemView.FindViewById<ImageView>(Resource.Id.trip_suggestion_stitched_images);
			Cost = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_cost);
			AddComment = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_add_comment);
			AddCommentInput = itemView.FindViewById<EditText>(Resource.Id.trip_suggestion_comment_edit_text);
			AddCommentButton = itemView.FindViewById<Button>(Resource.Id.trip_suggestion_add_comment_button);
			ShowComments = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_show_comments);
			CommentsListView = itemView.FindViewById<LinearLayout>(Resource.Id.trip_suggestion_comment_list);
			Menu = itemView.FindViewById<ImageView>(Resource.Id.trip_suggestion_menu);

			itemView.Click += (sender, e) => listener(AdapterPosition);
		}
	}

	public interface DeleteTripListener
	{
		void OnDeleteTrip(Trip trip);
	}
}
