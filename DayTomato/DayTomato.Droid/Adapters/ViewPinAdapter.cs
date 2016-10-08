﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	public class ViewPinAdapter : RecyclerView.Adapter
	{
		private List<Pin> _pins;
		private List<bool> _pinLiked;
		private List<bool> _pinDisliked;
		private Activity _context;

		public ViewPinAdapter(List<Pin> pins, Activity context)
		{
			_pins = pins;
			_pinLiked = new List<bool>(new bool[pins.Count]);
			_pinDisliked = new List<bool>(new bool[pins.Count]);
			_context = context;
		}

		public override int ItemCount
		{
			get { return _pins.Count; }
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			View itemView = LayoutInflater.From(parent.Context).
						Inflate(Resource.Layout.pin_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			ViewPinViewHolder vh = new ViewPinViewHolder(itemView);
			return vh;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			ViewPinViewHolder vh = holder as ViewPinViewHolder;

			//vh.PinImage.SetImageResource(imagesource);
			vh.PinName.Text = _pins[position].Name;
			vh.PinLikes.Text = _pins[position].Likes.ToString();
			vh.PinDescription.Text = _pins[position].Description;
			vh.PinReview.Text = _pins[position].Review;
			vh.PinLinkedAccount.Text = _pins[position].LinkedAccount;

			// Initializing listview
			vh.CommentsAdapter = new ViewPinCommentsAdapter(_context, _pins[position].Comments);
			// Make sure we can see the comments
			vh.CommentsListView.RemoveAllViews();
			for (int i = 0; i < vh.CommentsAdapter.Count; i++)
			{
				View v = vh.CommentsAdapter.GetView(i, null, vh.CommentsListView);
				vh.CommentsListView.AddView(v);
			}

			// When clicking add comment, show an edit text
			vh.AddComment.Click += (sender, e) => 
			{
				vh.AddCommentInput.Visibility = ViewStates.Visible;
				vh.AddCommentButton.Visibility = ViewStates.Visible;
			};

			// When the user presses add, add the new comment
			vh.AddCommentButton.Click += async (sender, e) => 
			{
				vh.AddCommentInput.Visibility = ViewStates.Gone;
				vh.AddCommentButton.Visibility = ViewStates.Gone;

				Account account = MainActivity.GetAccount();
				await MainActivity.dayTomatoClient.AddCommentToPin(_pins[position],
				                                                   vh.AddCommentInput.Text,
				                                                   account.Id);
				_pins[position].Comments.Add(new Comment(account.Id, vh.AddCommentInput.Text, DateTime.Today));
				this.NotifyDataSetChanged();
			};

			vh.UpButton.Click += (sender, e) =>
			{
				// If the like and dislike button was not pressed, then its fresh
				if (!_pinLiked[position] && !_pinDisliked[position])
				{
					_pinLiked[position] = true;
					_pinDisliked[position] = false;
					_pins[position].Likes++;
					vh.PinLikes.Text = _pins[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
				}
				// Else we need to "reset" the likes
				else if (_pinDisliked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = false;
					_pins[position].Likes++;
					vh.PinLikes.Text = _pins[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
				}
			};
			vh.DownButton.Click += (sender, e) =>
			{
				// If the like and dislike button was not pressed, then its fresh
				if (!_pinLiked[position] && !_pinDisliked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = true;
					_pins[position].Likes--;
					vh.PinLikes.Text = _pins[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
				}
				// Else we need to "reset" the likes
				else if (_pinLiked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = false;
					_pins[position].Likes--;
					vh.PinLikes.Text = _pins[position].Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
				}
			};
		}
	}

	public class ViewPinViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public ImageView PinImage { get; private set; }
		public TextView PinName { get; private set; }
		public ImageView UpButton { get; private set; }
		public TextView PinLikes { get; private set; }
		public ImageView DownButton { get; private set; }
		public TextView PinDescription { get; private set; }
		public TextView PinReview { get; private set; }
		public TextView PinLinkedAccount { get; private set; }
		public TextView AddComment { get; private set; }
		public EditText AddCommentInput { get; private set; }
		public Button AddCommentButton { get; private set; }

		public LinearLayout CommentsListView { get; set; }
		public ViewPinCommentsAdapter CommentsAdapter { get; set; }

		public ViewPinViewHolder(View itemView) : base(itemView)
		{
			PinImage = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_pin_image);
			PinName = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_pin_name);
			UpButton = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_up_button);
			PinLikes = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_likes);
			DownButton = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_down_button);
			PinDescription = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_description);
			PinReview = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_review);
			PinLinkedAccount = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_account);
			CommentsListView = itemView.FindViewById<LinearLayout>(Resource.Id.pin_view_holder_comment_list);
			AddComment = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_add_comment);
			AddCommentInput = itemView.FindViewById<EditText>(Resource.Id.pin_view_holder_comment_edit_text);
			AddCommentButton = itemView.FindViewById<Button>(Resource.Id.pin_view_holder_add_comment_button);
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
