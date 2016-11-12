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

namespace DayTomato.Droid.Adapters
{
	public class ViewPinAdapter : RecyclerView.Adapter
	{
	    private const string Tag = "VIEW_PIN_ADAPTER";
	    private List<Pin> _pins;
		private List<bool> _pinLiked;
		private List<bool> _pinDisliked;
		private readonly Activity _context;
		private readonly ViewPinDialogFragment _parent;
		private readonly Account _account;

		public ViewPinAdapter(List<Pin> pins, Activity context)
		{
			_pins = pins;
			_pinLiked = new List<bool>(new bool[_pins.Count]);
			_pinDisliked = new List<bool>(new bool[_pins.Count]);
			_context = context;
			_account = MainActivity.GetAccount();
		}

        public ViewPinAdapter(List<Pin> pins, Activity context, ViewPinDialogFragment parent) : this(pins, context)
        {
			_account = MainActivity.GetAccount();
            _parent = parent;
        }

        public IEnumerable<Pin> GetItems()
		{
			return _pins;
		}

		public override int ItemCount => _pins.Count;

	    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			var itemView = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.pin_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			return new ViewPinViewHolder(itemView);
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

		public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
            var vh = (ViewPinViewHolder)holder;
		    var pin = _pins[position]; 

            // Pin imageURL 
		    var imageUrl = pin.ImageURL;
			if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.Equals("none"))
			{
				try
				{
					var imageBytes = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(imageUrl);
					var imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
					vh.PinImage.SetImageBitmap(imageBitmap);
				}
				catch(Exception ex)
				{
					Log.Error(Tag, ex.Message);
				}
			}

			vh.PinName.Text = pin.Name;
			vh.PinLikes.Text = pin.Likes.ToString();
			try
			{
				if (pin.LikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					_pinLiked[position] = true;
					_pinDisliked[position] = false;

				}
				else if (pin.DislikedBy.Contains(_account.Id))
				{
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					_pinLiked[position] = false;
					_pinDisliked[position] = true;
				}
			}
			catch (Exception ex)
			{
				Log.Debug(Tag, ex.Message);
			}

			vh.PinDescription.Text = pin.Description;
			vh.PinReview.Text = pin.Review;
			vh.PinLinkedAccount.Text = pin.LinkedAccount;
            vh.PinRating.Rating = pin.Rating;
		    vh.PinRatingText.Text = pin.Rating.ToString();

            vh.PinCost.Text = pin.Cost > 0.0 ? $"${pin.Cost}" : "FREE";

			// Initializing listview
			vh.CommentsAdapter = new CommentsAdapter(_context, pin.Comments);
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
			vh.AddCommentButton.Click += async (sender, e) => 
			{
				vh.AddCommentInput.Visibility = ViewStates.Gone;
				vh.AddCommentButton.Visibility = ViewStates.Gone;

				if (pin.Comments.Count > 0 && pin.Comments.Last().Text == vh.AddCommentInput.Text)
					return; 
				pin.Comments.Add(new Comment(_account.Id, vh.AddCommentInput.Text, DateTime.Today));
				await MainActivity.dayTomatoClient.AddCommentToPin(pin, vh.AddCommentInput.Text, _account.Id);
				RefreshComments(vh.CommentsListView, vh.CommentsAdapter);
				vh.HideComments = !vh.HideComments;
				vh.CommentsListView.Visibility = ViewStates.Visible;
				vh.ShowComments.Text = "hide comments";
				vh.AddCommentInput.Text = "";
			};

			vh.ShowComments.Click += (sender, args) =>
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
				if (!_pinLiked[position] && !_pinDisliked[position])
				{
					_pinLiked[position] = true;
					_pinDisliked[position] = false;
					pin.Likes++;
					vh.PinLikes.Text = pin.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_filled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (!pin.LikedBy.Contains(_account.Id) && !pin.DislikedBy.Contains(_account.Id))
					{
						pin.LikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.LikePin(pin.Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_pinDisliked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = false;
					pin.Likes++;
					vh.PinLikes.Text = pin.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (pin.DislikedBy.Remove(_account.Id))
					{
						await MainActivity.dayTomatoClient.RemoveVotePin(pin.Id, _account);
					}
				}
			};
			vh.DownButton.Click += async (sender, e) =>
			{
				// If the like and dislike button was not pressed, then its fresh
				if (!_pinLiked[position] && !_pinDisliked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = true;
					pin.Likes--;
					vh.PinLikes.Text = pin.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_filled);
					if (!pin.LikedBy.Contains(_account.Id) && !pin.DislikedBy.Contains(_account.Id))
					{
						pin.DislikedBy.Add(_account.Id);
						await MainActivity.dayTomatoClient.DislikePin(pin.Id, _account);
					}
				}
				// Else we need to "reset" the likes
				else if (_pinLiked[position])
				{
					_pinLiked[position] = false;
					_pinDisliked[position] = false;
					pin.Likes--;
					vh.PinLikes.Text = pin.Likes.ToString();
					vh.UpButton.SetImageResource(Resource.Drawable.up_arrow_unfilled);
					vh.DownButton.SetImageResource(Resource.Drawable.down_arrow_unfilled);
					if (pin.LikedBy.Remove(_account.Id))
					{
						await MainActivity.dayTomatoClient.RemoveVotePin(pin.Id, _account);
					}
				}
			};
			if (pin.LinkedAccount == _account.Id)
			{
				vh.ViewMenu.Visibility = ViewStates.Visible;
				vh.ViewMenu.Click += (sender, e) =>
			   	{
				   	var menu = new Android.Support.V7.Widget.PopupMenu(_context, vh.ViewMenu, (int)GravityFlags.End);
				   	menu.Inflate(Resource.Menu.view_pin_popup_menu);

				   	menu.MenuItemClick += async (s1, arg1) =>
				   	{
					   	var command = arg1.Item.TitleFormatted.ToString();
					   	if (command.Equals("Delete"))
					   	{
							await MainActivity.dayTomatoClient.DeletePin(pin);
						   	_pins.RemoveAt(position);
						   	NotifyItemRemoved(position);
						   	NotifyDataSetChanged();
						}
                        else if (command.Equals("Edit"))
                        {
                            _parent.EditPinDialog(pin.Id, position);
                        }
                    };
					menu.Show();
				};
			}
		}
    }

	public class ViewPinViewHolder : RecyclerView.ViewHolder
	{
		public ImageView PinImage { get; }
		public TextView PinName { get; }
		public ImageView UpButton { get; }
		public TextView PinLikes { get; }
		public ImageView DownButton { get; }
		public TextView PinDescription { get; }
		public TextView PinReview { get; }
        public TextView PinCost { get; }
        public RatingBar PinRating { get; }
		public TextView PinRatingText { get; }
		public TextView PinLinkedAccount { get; }
		public TextView AddComment { get; }
		public EditText AddCommentInput { get; }
		public Button AddCommentButton { get; }
		public TextView ShowComments { get; }
		public ImageView ViewMenu { get; }
		public bool HideComments { get; set; }
		public LinearLayout CommentsListView { get; }
		public CommentsAdapter CommentsAdapter { get; set; }

		public ViewPinViewHolder(View itemView) : base(itemView)
		{
			PinImage = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_pin_image);
			PinName = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_pin_name);
			UpButton = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_up_button);
			PinLikes = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_likes);
			DownButton = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_down_button);
			PinDescription = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_description);
			PinReview = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_review);
            PinCost = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_cost);
            PinRating = itemView.FindViewById<RatingBar>(Resource.Id.pin_view_holder_rating);
			PinRatingText = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_rating_text);
			PinLinkedAccount = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_account);
			CommentsListView = itemView.FindViewById<LinearLayout>(Resource.Id.pin_view_holder_comment_list);
			AddComment = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_add_comment);
			AddCommentInput = itemView.FindViewById<EditText>(Resource.Id.pin_view_holder_comment_edit_text);
			AddCommentButton = itemView.FindViewById<Button>(Resource.Id.pin_view_holder_add_comment_button);
			ShowComments = itemView.FindViewById<TextView>(Resource.Id.pin_view_holder_show_comments);
			ViewMenu = itemView.FindViewById<ImageView>(Resource.Id.pin_view_holder_view_menu);
			HideComments = true;
		}
	}
}
