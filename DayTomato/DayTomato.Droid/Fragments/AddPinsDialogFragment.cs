using System;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DayTomato.Droid
{
    public class AddPinsDialogFragment : DialogFragment
    {

        public event EventHandler<AddPinsDialogEventArgs> AddPinsDialogClosed;      // Event handler when user presses create
        private Button _createTripButton;                               // Create trip button
        private Button _cancelButton;                                   // Cancel create trip button
        private Button _addPinButton;                                   // Add pin to trip button
		private List<Pin> _addedPins;                                   // Array of added pins
		private List<String> _addedPinsIds;
        private List<Pin> _allPins;                                     // All pins currently on server
		private AutoCompleteTextView _autocompleteTextView;             // Search for pins
        private TextView _listPins;                                     // List of added pins
        private bool _createTrip;                                       // Check if they pressed create or not

        public static AddPinsDialogFragment NewInstance()
        {
            AddPinsDialogFragment fragment = new AddPinsDialogFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.add_pins_dialog_fragment, container, false);
            _createTripButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_create_button);
            _cancelButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_cancel_button);
			_addPinButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_add_button);
            _listPins = view.FindViewById<TextView>(Resource.Id.add_pins_dialog_listpins);
			_addedPins = new List<Pin>();
			_addedPinsIds = new List<string>();

			ArrayAdapter autoCompleteAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            _autocompleteTextView = view.FindViewById<AutoCompleteTextView>(Resource.Id.add_pins_dialog_autocomplete);
            // Minimum number of characters to begin autocompletet for
            _autocompleteTextView.Threshold = 1;
            _autocompleteTextView.Adapter = autoCompleteAdapter;

            // Load all pins from the server
            Task.Run(async () =>
            {
                _allPins = await MainActivity.dayTomatoClient.GetPins();
                autoCompleteAdapter.AddAll(_allPins.Select(p => p.Name).ToList());
                // Updates autocomplete data
                autoCompleteAdapter.NotifyDataSetChanged();
            });

            Dialog.SetCancelable(true);
            Dialog.SetCanceledOnTouchOutside(true);
            
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
            if (AddPinsDialogClosed != null && _createTrip)
            { 
                AddPinsDialogClosed(this, new AddPinsDialogEventArgs
                {
                    PinsIds = _addedPinsIds,
					Pins = _addedPins
                });
            }
        }

        private void SetListeners()
        {
			// Anytime the user clicks on the add button, add that pin to the trip and display the list of pins on the screen
			_addPinButton.Click += (sender, e) =>
			{
				if (_autocompleteTextView.Text != null)
				{
					Pin _pin = _allPins.Find(p => p.Name == _autocompleteTextView.Text);
					if (_pin != null)
					{
						_addedPins.Add(_pin);
						_addedPinsIds.Add(_pin.Id);
						Toast.MakeText(Activity, "Added Pin", ToastLength.Short).Show();
						_listPins.Text += _autocompleteTextView.Text + "\n";
						_pin = null;
						_autocompleteTextView.Text = "";
					}
					else { Toast.MakeText(Activity, "Could not find pin", ToastLength.Short).Show(); }
				}
				else { Toast.MakeText(Activity, "Type the name of a pin to add", ToastLength.Short).Show(); }
			};

            _createTripButton.Click += (sender, e) =>
            {
                Toast.MakeText(Activity, "Created Trip", ToastLength.Short).Show();
                _createTrip = true;
                Dialog.Dismiss();
            };

            _cancelButton.Click += (sender, e) =>
            {
                _createTrip = false;
                Dialog.Dismiss();
            };
        }
        }
    public class AddPinsDialogEventArgs
    {
        public List<string> PinsIds { get; set; }
		public List<Pin> Pins { get; set; }
    }
}