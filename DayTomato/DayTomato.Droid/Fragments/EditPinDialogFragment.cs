using System;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Util;
using Plugin.Media;
using System.IO;
using DayTomato.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using System.Text.RegularExpressions;
using Android.App;

namespace DayTomato.Droid
{
	public class EditPinDialogFragment : DialogFragment
	{
		private readonly static string TAG = "EDIT_PIN_DIALOG_FRAGMENT";

		public event EventHandler<EditPinDialogEventArgs> EditPinDialogClosed;		// Event handler when user presses create
		private Button _finishButton;									// finish editting pin button
		private Button _cancelButton;									// Cancel editting pin button
		private ImageView _image;										// image for pin
		private EditText _name;											// Name user will put
		private EditText _description;									// Description user will put
		private RatingBar _rating;                                      // Rating user will give
		private EditText _review;										// Review user will give
        private EditText _cost;                                         // Amount user spent
		private bool _editPin;                                        	// Check if they pressed done or cancel
		private string _imageUrl;										// Pin imageurl
        private Pin _pin;                                               // Pin to be edited
		private int _pinPosition;										// Edit pin position

		public static EditPinDialogFragment NewInstance(Bundle bundle)
		{
			EditPinDialogFragment fragment = new EditPinDialogFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.edit_pin_dialog_fragment, container, false);
			_finishButton = (Button)view.FindViewById(Resource.Id.edit_pin_dialog_create_button);
			_cancelButton = (Button)view.FindViewById(Resource.Id.edit_pin_dialog_cancel_button);
			_image = (ImageView)view.FindViewById(Resource.Id.edit_pin_dialog_image);
			_name = (EditText)view.FindViewById(Resource.Id.edit_pin_dialog_name);
			_description = (EditText)view.FindViewById(Resource.Id.edit_pin_dialog_text_description);
			_rating = (RatingBar)view.FindViewById(Resource.Id.edit_pin_dialog_rating);
			_review = (EditText)view.FindViewById(Resource.Id.edit_pin_dialog_review);
            _cost = (EditText)view.FindViewById(Resource.Id.edit_pin_dialog_cost);
            
			this.Dialog.SetCancelable(true);
			this.Dialog.SetCanceledOnTouchOutside(true);

			SetInstances();
			SetListeners();
			return view;
		}

		public override void OnResume()
		{
			base.OnResume();
			Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
		}

		public override async void OnDismiss(IDialogInterface dialog)
		{
			base.OnDismiss(dialog);

			// Store and output data to the parent fragment
			if (EditPinDialogClosed != null && _editPin)
			{
				EditPinDialogClosed(this, new EditPinDialogEventArgs
				{
					Id = _pin.Id,
					Name = _name.Text,
					Description = _description.Text,
					Latitude = _pin.Coordinate.latitude,
					Longitude = _pin.Coordinate.longitude,
					Rating = _rating.Rating,
					Review = _review.Text,
					Type = _pin.Type,
					Likes = _pin.Likes,
					Cost = Convert.ToDouble(_cost.Text),
					CreateDate = _pin.CreateDate,
					ImageUrl = _imageUrl,
					Comments = _pin.Comments,
					LikedBy = _pin.LikedBy,
					DislikedBy = _pin.DislikedBy,
					PinPosition = _pinPosition
				});
            }
		}

		private async void SetInstances()
		{
			_pin = await MainActivity.dayTomatoClient.GetPin(Arguments.GetString("EDIT_PIN_ID"));
			_pinPosition = Arguments.GetInt("EDIT_PIN_POSITION");

			_name.Text = _pin.Name;
			_description.Text = _pin.Description;
			_rating.Rating = _pin.Rating;
			_review.Text = _pin.Review;
			_cost.Text = _pin.Cost.ToString();
			_imageUrl = _pin.ImageURL;

			try
			{
				byte[] image = await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(_imageUrl);
				Bitmap bmp = BitmapFactory.DecodeByteArray(image, 0, image.Length);
				_image.SetImageBitmap(bmp);
			}
			catch (Exception ex)
			{
				Log.Error(TAG, ex.Message);
			}
		}

		private void SetListeners()
		{
			_finishButton.Click += (sender, e) =>
			{
				if (!isValidCost(_cost.Text))
				{
					_cost.Error = "Cannot Be Empty";
					_editPin = false;

				}
				else {
					Toast.MakeText(this.Activity, "Created Pin", ToastLength.Short).Show();
					_editPin = true;
					Dialog.Dismiss();
				}
			};

			_cancelButton.Click += (sender, e) =>
			{
				_editPin = false;
				Dialog.Dismiss();
			};

			_image.Click += (sender, e) => 
			{
				PopupWindow menu = new PopupWindow(Activity);
				var adapter = new ArrayAdapter<string>(Activity, 
				                                       Android.Resource.Layout.SimpleListItem1, 
				                                       new string[] { "Choose Photo", "Take Photo" });
				ListView list = new ListView(Activity) { Adapter = adapter };
				list.ItemClick += (s, args) =>
				{
					switch (args.Position)
					{
						case 0:
							ChoosePhoto();
							break;
						case 1:
							TakePhoto();
							break;
					}
					menu.Dismiss();
				};
				menu.Width = ViewGroup.LayoutParams.WrapContent;
				menu.Height = ViewGroup.LayoutParams.WrapContent;
				menu.ContentView = list;
				menu.ShowAtLocation(View, GravityFlags.Center, 0, 0);
			};	
		}

		private bool isValidCost(string cost)
		{
			Regex regex = new Regex(@"[0-9]+");
			return regex.IsMatch(cost);
		}

		private async void ChoosePhoto()
		{
			await CrossMedia.Current.Initialize();
			if (!CrossMedia.Current.IsPickPhotoSupported)
			{
				Toast.MakeText(this.Activity, "Cannot choose photos", ToastLength.Short);
				return;
			}

			Android.App.ProgressDialog pd = new Android.App.ProgressDialog(this.Activity);
			pd.Show();
			pd.SetMessage("Loading...");

			var file = await CrossMedia.Current.PickPhotoAsync();

			if (file == null)
			{
				return;
			}

			var resizedBitmap = await PictureUtil.DecodeByteArrayAsync(file.Path, 200, 200);

			var stream = new MemoryStream();
			resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
			var resizedImg = stream.ToArray();

			var imgurl = await MainActivity.imgurClient.UploadImage(resizedImg);
			_imageUrl = imgurl;
			_image.SetImageBitmap(resizedBitmap);
			pd.Hide();
		}

		private async void TakePhoto()
		{
			await CrossMedia.Current.Initialize();
			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				Toast.MakeText(this.Activity, "No Camera available", ToastLength.Short);
				return;
			}

			Android.App.ProgressDialog pd = new Android.App.ProgressDialog(this.Activity);
			pd.Show();
			pd.SetMessage("Loading...");

			var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
			{
				Directory = "DayTomato",
				Name = string.Format("{0}.jpg", Guid.NewGuid()),
				SaveToAlbum = true,
				PhotoSize = Plugin.Media.Abstractions.PhotoSize.Small,
				CompressionQuality = 92
			});

			if (file == null)
			{
				return;
			}

			var resizedBitmap = await PictureUtil.DecodeByteArrayAsync(file.AlbumPath, 200, 200);

			var stream = new MemoryStream();
			resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
			var resizedImg = stream.ToArray();

			var imgurl = await MainActivity.imgurClient.UploadImage(resizedImg);
			_imageUrl = imgurl;
			_image.SetImageBitmap(resizedBitmap);
			pd.Hide();
		}
	}

	public class EditPinDialogEventArgs
	{
        public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
		public float Rating { get; set; }
		public string Review { get; set; }
        public int Type { get; set; }
        public int Likes { get; set; }
        public double Cost { get; set; }
        public DateTime CreateDate { get; set; }
        public string ImageUrl { get; set; }
		public List<Comment> Comments { get; set; }
		public List<string> LikedBy { get; set; }
		public List<string> DislikedBy { get; set; }
		public int PinPosition { get; set; }
	}
}
