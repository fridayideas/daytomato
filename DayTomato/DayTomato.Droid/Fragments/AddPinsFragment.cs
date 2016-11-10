
using Android.OS;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Android.App;

namespace DayTomato.Droid
{
    public class AddPinsFragment : Fragment
    {

		private Button _addPin;
		private List<Pin> _addedPins;                                   // Array of added pins
		private List<string> _addedPinsIds;
        private List<Pin> _allPins;                                     // All pins currently on server
		private AutoCompleteTextView _autocompleteTextView;             // Search for pins
        private TextView _listPins;                                     // List of added pins

        public static AddPinsFragment NewInstance()
        {
            AddPinsFragment fragment = new AddPinsFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.add_pins_fragment, container, false);
            _listPins = view.FindViewById<TextView>(Resource.Id.add_pins_dialog_listpins);
			_addPin = view.FindViewById<Button>(Resource.Id.add_pins_dialog_add_button);
			_addedPins = new List<Pin>();
			_addedPinsIds = new List<string>();

			SetListeners();

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

            return view;
        }

		private void SetListeners()
		{
			// Anytime the user clicks on the add button, add that pin to the trip and display the list of pins on the screen
			_addPin.Click += (sender, e) =>
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
		}

        public List<Pin> FinalizePins()
        {
			// Store and output data to the parent fragment
			return _addedPins;
        }
    }
}