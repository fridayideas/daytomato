
using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	public class ViewTripAdapter : RecyclerView.Adapter
	{
		//Create an Event so that our our clients can act when a user clicks
		//on each individual item.
		public event EventHandler<int> HandleClick;

		private readonly string TAG = "VIEW_TRIP_ADAPTER";
		private List<Trip> _suggestions;
		private List<bool> _tripLiked;
		private List<bool> _tripDisliked;
		private Activity _context;
		private Account _account;

		public ViewTripAdapter(List<Trip> suggestions, Activity context)
		{
			_suggestions = suggestions;
			_tripLiked = new List<bool>(new bool[_suggestions.Count]);
			_tripDisliked = new List<bool>(new bool[_suggestions.Count]);
			_context = context;
			_account = MainActivity.GetAccount();
		}

		public override int ItemCount
		{
			get { return _suggestions.Count; }
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			View itemView = LayoutInflater.From(parent.Context).
						Inflate(Resource.Layout.trip_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			TripSuggestionViewHolder vh = new TripSuggestionViewHolder(itemView, OnClick);
			return vh;
		}

		public void RefreshComments(LinearLayout ll, CommentsAdapter ca)
		{
			ll.RemoveAllViews();
			for (int i = 0; i < ca.Count; i++)
			{
				View v = ca.GetView(i, null, ll);
				ll.AddView(v);
			}
			ca.NotifyDataSetChanged();
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			TripSuggestionViewHolder vh = holder as TripSuggestionViewHolder;
			vh.Name.Text = _suggestions[position].Name;
			vh.Type.Text = _suggestions[position].Type;
			vh.Pins.Text = "";
			foreach (var p in _suggestions[position].Pins)
			{
				vh.Pins.Text += p.Name + "\n";
			}
			vh.CreateDate.Text = "created " + _suggestions[position].CreateDate.ToLongDateString();
			vh.Account.Text = _suggestions[position].LinkedAccount;
			vh.Likes.Text = _suggestions[position].Likes.ToString();
			vh.Description.Text = _suggestions[position].Description;
			SetCost(vh, _suggestions[position].Cost);
			SetImage(vh, _suggestions[position].Pins);

			try
			{
				if (_suggestions[position].LikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					_tripLiked[position] = true;
					_tripDisliked[position] = false;

				}
				else if (_suggestions[position].DislikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					_tripLiked[position] = false;
					_tripDisliked[position] = true;
				}
			}
			catch (Exception ex)
			{
				Log.Debug(TAG, ex.Message);
			}

			// Initializing listview
			vh.CommentsAdapter = new CommentsAdapter(_context, _suggestions[position].Comments);
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

				Account account = MainActivity.GetAccount();
				if (_suggestions[position].Comments.Count > 0 && _suggestions[position].Comments[_suggestions[position].Comments.Count - 1].Text == vh.AddCommentInput.Text)
					return;
				_suggestions[position].Comments.Add(new Comment(account.Id, vh.AddCommentInput.Text, DateTime.Today));
				//await MainActivity.dayTomatoClient.AddCommentToTrip(_suggestions[position],
				//												   vh.AddCommentInput.Text,
				//												   account.Id);
				RefreshComments(vh.CommentsListView, vh.CommentsAdapter);
				vh.HideComments = !vh.HideComments;
				vh.CommentsListView.Visibility = ViewStates.Visible;
				vh.ShowComments.Text = "hide comments";
				vh.AddCommentInput.Text = "";
			};
			vh.ShowComments.Click += (sender, e) =>
			{
				vh.HideComments = !vh.HideComments;
				if (vh.HideComments)
				{
					vh.CommentsListView.RemoveAllViews();
					vh.CommentsListView.Visibility = ViewStates.Gone;
					vh.ShowComments.Text = "show comments";
					vh.CommentsAdapter.NotifyDataSetChanged();
				}
				else
				{
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
					_suggestions[position].Likes++;
					vh.Likes.Text = _suggestions[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (!_suggestions[position].LikedBy.Contains(_account.Id) && !_suggestions[position].DislikedBy.Contains(_account.Id))
					{
						_suggestions[position].LikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.LikeTrip(_suggestions[position].Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_tripDisliked[position])
				{
					_tripLiked[position] = false;
					_tripDisliked[position] = false;
					_suggestions[position].Likes++;
					vh.Likes.Text = _suggestions[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (_suggestions[position].DislikedBy.Contains(_account.Id))
					{
						_suggestions[position].DislikedBy.Remove(_account.Id);
						await MainActivity.dayTomatoClient.RemoveVoteTrip(_suggestions[position].Id, _account);
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
					_suggestions[position].Likes--;
					vh.Likes.Text = _suggestions[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					if (!_suggestions[position].LikedBy.Contains(_account.Id) && !_suggestions[position].DislikedBy.Contains(_account.Id))
					{
						_suggestions[position].DislikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.DislikeTrip(_suggestions[position].Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_tripLiked[position])
				{
					_tripLiked[position] = false;
					_tripDisliked[position] = false;
					_suggestions[position].Likes--;
					vh.Likes.Text = _suggestions[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (_suggestions[position].LikedBy.Contains(_account.Id))
					{
						_suggestions[position].LikedBy.Remove(_account.Id);
						await MainActivity.dayTomatoClient.RemoveVoteTrip(_suggestions[position].Id, _account);
					}
				}
			};
		}

		private void SetCost(TripSuggestionViewHolder vh, double cost)
		{
			if (cost > 0.0)
			{
				vh.Cost.Text = "$" + cost;
			}
			else
			{
				vh.Cost.Text = "FREE";
			}
		}

		private async void SetImage(TripSuggestionViewHolder vh, List<Pin> pins)
		{
			List<string> imageUrls = pins.Select(p => p.ImageURL).ToList();
			Bitmap[] bitmaps = new Bitmap[imageUrls.Count];

			for (int i = 0; i < bitmaps.Length; ++i)
			{
				var imageUrl = imageUrls[i];
				if (!imageUrl.Equals("none") && !imageUrl.Equals("") && imageUrl != null)
				{
					try
					{
						var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
						var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
						bitmaps[i] = imageBitmap;
					}
					catch (Exception ex)
					{
						Log.Error(TAG, ex.Message);
					}
				}
			}
			try
			{
				Bitmap bitmap = PictureUtil.StitchImages(bitmaps);
				vh.StitchedImages.SetImageBitmap(bitmap);
			}
			catch (Exception ex)
			{
				Log.Error(TAG, ex.Message);
				vh.StitchedImages.SetImageBitmap(null);
			}
		}

		//This will fire any event handlers that are registered with our ItemClick
		//event.
		private void OnClick(int position)
		{
			if (HandleClick != null)
			{
				HandleClick(this, position);
			}
		}
	}

	public class TripSuggestionViewHolder : RecyclerView.ViewHolder
	{
		public TextView Name { get; private set; }
		public TextView Type { get; private set; }
		public TextView Pins { get; private set; }
		public TextView CreateDate { get; private set; }
		public TextView Account { get; private set; }
		public ImageView UpButton { get; private set; }
		public TextView Likes { get; private set; }
		public ImageView DownButton { get; private set; }
		public TextView Description { get; private set; }
		public ImageView StitchedImages { get; private set; }
		public TextView Cost { get; private set; }
		public TextView AddComment { get; private set; }
		public EditText AddCommentInput { get; private set; }
		public Button AddCommentButton { get; private set; }
		public TextView ShowComments { get; private set; }
		public ImageView ViewMenu { get; private set; }
		public bool HideComments { get; set; }
		public LinearLayout CommentsListView { get; set; }
		public CommentsAdapter CommentsAdapter { get; set; }

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

			itemView.Click += (sender, e) => listener(AdapterPosition);
		}
	}
}
