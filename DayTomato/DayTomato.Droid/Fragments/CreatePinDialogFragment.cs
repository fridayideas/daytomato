﻿
using System;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Util;
using Plugin.Media;
using System.IO;

namespace DayTomato.Droid
{
	public class CreatePinDialogFragment : DialogFragment
	{
		private readonly static string TAG = "CREATE_PIN_DIALOG_FRAGMENT";

		public event EventHandler<CreatePinDialogEventArgs> CreatePinDialogClosed;		// Event handler when user presses create
		private Button _createPinButton;								// Create pin button
		private Button _cancelButton;									// Cancel create pin button
		private ImageView _image;										// TODO: Allow user to take photos
		private TextView _selectedLocationText;							// Location that reverse geocoding got
		private EditText _name;											// Name user will put
		private EditText _description;									// Description user will put
		private RatingBar _rating;                                      // Rating user will give
		private EditText _review;										// Review user will give
        private EditText _cost;                                         // Amount user spent
		private bool _createPin;                                        // Check if they pressed create or not
		private string _imageUrl;

		public static CreatePinDialogFragment NewInstance(Bundle bundle)
		{
			CreatePinDialogFragment fragment = new CreatePinDialogFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.create_pin_dialog_fragment, container, false);
			_createPinButton = (Button)view.FindViewById(Resource.Id.create_pin_dialog_create_button);
			_cancelButton = (Button)view.FindViewById(Resource.Id.create_pin_dialog_cancel_button);
			_image = (ImageView)view.FindViewById(Resource.Id.create_pin_dialog_image);
			_selectedLocationText = (TextView)view.FindViewById(Resource.Id.create_pin_dialog_location);
			_name = (EditText)view.FindViewById(Resource.Id.create_pin_dialog_name);
			_description = (EditText)view.FindViewById(Resource.Id.create_pin_dialog_text_description);
			_rating = (RatingBar)view.FindViewById(Resource.Id.create_pin_dialog_rating);
			_review = (EditText)view.FindViewById(Resource.Id.create_pin_dialog_review);
            _cost = (EditText)view.FindViewById(Resource.Id.create_pin_dialog_cost);

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

		public override void OnDismiss(IDialogInterface dialog)
		{
			base.OnDismiss(dialog);

			// Store and output data to the parent fragment
			if (CreatePinDialogClosed != null && _createPin)
			{
				CreatePinDialogClosed(this, new CreatePinDialogEventArgs
				{
					Name = _name.Text,
					Description = _description.Text,
					Rating = _rating.Rating,
					Review = _review.Text,
					Cost = Convert.ToDouble(_cost.Text),
					Location = new LatLng(Arguments.GetDouble("SELECTED_LOCATION_LATITUDE"),
										  Arguments.GetDouble("SELECTED_LOCATION_LONGITUDE")),
					CreateDate = DateTime.Today,
					ImageUrl = _imageUrl
				});

				MainActivity.UpdateAccount(MainActivity.GetAccount().Id, 1, 1);
            }
		}

		private void SetInstances()
		{
			// Get the location that the user selected
			_selectedLocationText.Text = Arguments.GetString("SELECTED_LOCATION", "Unknown Location");
			_name.Text = Arguments.GetString("SELECTED_LOCATION_NAME", "");
			try
			{
				byte[] image = Arguments.GetByteArray("SELECTED_LOCATION_IMAGE");
				Bitmap bmp = BitmapFactory.DecodeByteArray(image, 0, image.Length);
				_image.SetImageBitmap(bmp);
			}
			catch (Exception ex)
			{
				Log.Error(TAG, ex.Message);
			}
			_description.Text = Arguments.GetString("SELECTED_LOCATION_DESCRIPTION", "");
			_imageUrl = "";
			_cost.Text = "0";
		}

		private void SetListeners()
		{
			_createPinButton.Click += (sender, e) =>
			{
				Toast.MakeText(this.Activity, "Created Pin", ToastLength.Short).Show();
				_createPin = true;
				Dialog.Dismiss();
			};

			_cancelButton.Click += (sender, e) =>
			{
				_createPin = false;
				Dialog.Dismiss();
			};
			_image.Click += async (sender, e) => 
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

				Toast.MakeText(this.Activity, "Photo saved: " + file.Path, ToastLength.Short);

				var resizedBitmap = await PictureUtil.DecodeByteArrayAsync(file.AlbumPath, 200, 200);

				var stream = new MemoryStream();
				resizedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
				var resizedImg = stream.ToArray();

				var imgurl = await MainActivity.dayTomatoClient.UploadImage(resizedImg);
				_imageUrl = imgurl;
				_image.SetImageBitmap(resizedBitmap);
				pd.Hide();
			};	
		}
	}

	public class CreatePinDialogEventArgs
	{
		public string Name { get; set; }
		public LatLng Location { get; set; }
		public string Description { get; set; }
		public float Rating { get; set; }
		public string Review { get; set; }
        public double Cost { get; set; }
		public DateTime CreateDate { get; set; }
		public string ImageUrl { get; set; }
	}
}
