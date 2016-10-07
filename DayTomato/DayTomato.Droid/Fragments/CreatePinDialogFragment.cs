
using System;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;

namespace DayTomato.Droid
{
	public class CreatePinDialogFragment : DialogFragment
	{
		public event EventHandler<CreatePinDialogEventArgs> CreatePinDialogClosed;		// Event handler when user presses create
		private Button _createPinButton;								// Create pin button
		private Button _cancelButton;									// Cancel create pin button
		private ImageView _image;										// TODO: Allow user to take photos
		private TextView _selectedLocationText;							// Location that reverse geocoding got
		private EditText _name;											// Name user will put
		private EditText _description;									// Description user will put
		private RatingBar _rating;                                      // Rating user will give
		private EditText _review;										// Review user will give
		private bool _createPin;										// Check if they pressed create or not

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
					Location = new LatLng(Arguments.GetDouble("SELECTED_LOCATION_LATITUDE"),
					                      Arguments.GetDouble("SELECTED_LOCATION_LONGITUDE")) 
				});
			}
		}

		private void SetInstances()
		{
			// Get the location that the user selected
			_selectedLocationText.Text = Arguments.GetString("SELECTED_LOCATION", "Unknown Location");
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
				
		}
	}

	public class CreatePinDialogEventArgs
	{
		public string Name { get; set; }
		public LatLng Location { get; set; }
		public string Description { get; set; }
		public float Rating { get; set; }
		public string Review { get; set; }
	}
}
