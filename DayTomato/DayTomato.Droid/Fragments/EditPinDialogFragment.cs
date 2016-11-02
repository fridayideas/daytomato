
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
using DayTomato.Models;
using System.Threading.Tasks;

namespace DayTomato.Droid
{
	public class EditPinDialogFragment : DialogFragment
	{
		private readonly static string TAG = "EDIT_PIN_DIALOG_FRAGMENT";

		public event EventHandler<EditPinDialogEventArgs> EditPinDialogClosed;		// Event handler when user presses create
		private Button _finishButton;								// Create pin button
		private Button _cancelButton;									// Cancel create pin button
		private ImageView _image;										// TODO: Allow user to take photos
		private EditText _name;											// Name user will put
		private EditText _description;									// Description user will put
		private RatingBar _rating;                                      // Rating user will give
		private EditText _review;										// Review user will give
        private EditText _cost;                                         // Amount user spent
		private bool _editPin;                                        // Check if they pressed create or not
		private string _imageUrl;
        private Pin pin;                                                // Pin to be edited

		public static EditPinDialogFragment NewInstance(string Id)
		{
			EditPinDialogFragment fragment = new EditPinDialogFragment();
            Bundle args = new Bundle();
            args.PutString("pin id", Id);
			fragment.Arguments = args;

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

		public override void OnDismiss(IDialogInterface dialog)
		{
			base.OnDismiss(dialog);

			// Store and output data to the parent fragment
			if (EditPinDialogClosed != null && _editPin)
			{
                EditPinDialogClosed(this, new EditPinDialogEventArgs
                {
                    Id = pin.Id,
                    Name = _name.Text,
                    Description = _description.Text,
                    Latitude = pin.Latitude,
                    Longitude = pin.Longitude,
					Rating = _rating.Rating,
					Review = _review.Text,
                    Type = pin.Type,
                    Likes = pin.Likes,
					Cost = Convert.ToDouble(_cost.Text),
                    CreateDate = pin.CreateDate,
                    ImageUrl = _imageUrl
				});

				MainActivity.UpdateAccount(MainActivity.GetAccount().Id, 1, 1);
            }
		}

		private async void SetInstances()
		{
            pin = await MainActivity.dayTomatoClient.GetPin(Arguments.GetString("pin id"));

            _name.Text = pin.Name;
            _description.Text = pin.Description;
            _rating.Rating = pin.Rating;
            _review.Text = pin.Review;
            _cost.Text = pin.Cost.ToString();
            _imageUrl = pin.ImageURL;
		}

		private void SetListeners()
		{
			_finishButton.Click += (sender, e) =>
			{
				Toast.MakeText(this.Activity, "Edited Pin", ToastLength.Short).Show();
				_editPin = true;
				Dialog.Dismiss();
			};

			_cancelButton.Click += (sender, e) =>
			{
				_editPin = false;
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
	}
}
