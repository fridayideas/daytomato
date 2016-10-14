using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	public class ViewTripAdapter : RecyclerView.Adapter
	{
		private List<Pin> _pins;
		private Activity _context;

		public ViewTripAdapter(List<Pin> pins, Activity context)
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

			View pin = inflater.
					Inflate(Resource.Layout.trip_pin_view_holder, parent, false);
			viewHolder = new TripPinViewHolder(pin);


			return viewHolder;
		}

		public async override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			TripPinViewHolder vh = holder as TripPinViewHolder;

			// Pin imageURL 
			var imageUrl = _pins[position].ImageURL;
			if (!imageUrl.Equals("none") && !imageUrl.Equals(""))
			{
				var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
				var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
				vh.PinImage.SetImageBitmap(imageBitmap);
			}

			vh.PinName.Text = _pins[position].Name;
			vh.PinLikes.Text = _pins[position].Likes.ToString() + " likes";
			vh.PinDescription.Text = _pins[position].Description;
			vh.PinReview.Text = _pins[position].Review;
			vh.PinLinkedAccount.Text = _pins[position].LinkedAccount;
			vh.PinRating.Text = "Rating: " + _pins[position].Rating.ToString();

			double cost = _pins[position].Cost;
			vh.PinCost.Text = "Cost: $" + cost;

			// Initializing listview
			vh.CommentsAdapter = new ViewPinCommentsAdapter(_context, _pins[position].Comments);
			// Make sure we can see the comments
			vh.CommentsListView.RemoveAllViews();
			if (!vh.HideComments)
			{
				for (int i = 0; i < vh.CommentsAdapter.Count; i++)
				{
					View v = vh.CommentsAdapter.GetView(i, null, vh.CommentsListView);
					vh.CommentsListView.AddView(v);
				}
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

			vh.ShowComments.Click += (sender, e) =>
			{
				vh.HideComments = !vh.HideComments;
				if (vh.HideComments)
				{
					vh.CommentsListView.RemoveAllViews();
					vh.CommentsListView.Visibility = ViewStates.Gone;
					vh.ShowComments.Text = "show comments";
				}
				else
				{
					vh.CommentsListView.Visibility = ViewStates.Visible;
					vh.ShowComments.Text = "hide comments";
					vh.CommentsListView.RemoveAllViews();
					for (int i = 0; i < vh.CommentsAdapter.Count; i++)
					{
						View v = vh.CommentsAdapter.GetView(i, null, vh.CommentsListView);
						vh.CommentsListView.AddView(v);
					}
				}
			};
		}
	}

	public class TripPinViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public ImageView PinImage { get; private set; }
		public TextView PinName { get; private set; }
		public TextView PinLikes { get; private set; }
		public TextView PinDescription { get; private set; }
		public TextView PinReview { get; private set; }
		public TextView PinCost { get; private set; }
		public TextView PinRating { get; private set; }
		public TextView PinLinkedAccount { get; private set; }
		public TextView AddComment { get; private set; }
		public EditText AddCommentInput { get; private set; }
		public Button AddCommentButton { get; private set; }
		public TextView ShowComments { get; private set; }
		public bool HideComments { get; set; }
		public LinearLayout CommentsListView { get; set; }


		public ViewPinCommentsAdapter CommentsAdapter { get; set; }

		public TripPinViewHolder(View itemView) : base(itemView)
		{
			PinImage = itemView.FindViewById<ImageView>(Resource.Id.trip_pin_view_holder_pin_image);
			PinName = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_pin_name);
			PinLikes = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_likes);
			PinDescription = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_description);
			PinReview = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_review);
			PinCost = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_cost);
			PinRating = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_rating);
			PinLinkedAccount = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_account);
			CommentsListView = itemView.FindViewById<LinearLayout>(Resource.Id.trip_pin_view_holder_comment_list);
			AddComment = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_add_comment);
			AddCommentInput = itemView.FindViewById<EditText>(Resource.Id.trip_pin_view_holder_comment_edit_text);
			AddCommentButton = itemView.FindViewById<Button>(Resource.Id.trip_pin_view_holder_add_comment_button);
			ShowComments = itemView.FindViewById<TextView>(Resource.Id.trip_pin_view_holder_show_comments);
			HideComments = true;
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
