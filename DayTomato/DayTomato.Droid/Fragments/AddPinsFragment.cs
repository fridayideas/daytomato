
using Android.OS;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Android.App;
using Android.Content;

namespace DayTomato.Droid
{
    public class AddPinsFragment : Fragment
    {

		private List<Pin> _addedPins;                                   // Array of added pins
		private List<string> _addedPinsIds;
        private List<Pin> _allPins;                                     // All pins currently on server
		private AutoCompleteTextView _autocompleteTextView;             // Search for pins
		private ArrayAdapter _autoCompleteAdapter;
        private TextView _listPins;                                     // List of added pins
		private TextView _createPin;

        public static AddPinsFragment NewInstance()
        {
            AddPinsFragment fragment = new AddPinsFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.add_pins_fragment, container, false);
            _listPins = view.FindViewById<TextView>(Resource.Id.add_pins_listpins);
			_addedPins = new List<Pin>();
			_addedPinsIds = new List<string>(); 
			_autoCompleteAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            _autocompleteTextView = view.FindViewById<AutoCompleteTextView>(Resource.Id.add_pins_autocomplete);
            _autocompleteTextView.Adapter = _autoCompleteAdapter;
			_createPin = view.FindViewById<TextView>(Resource.Id.add_pins_create_pin);

			SetListeners();

            // Load all pins from the server
            Task.Run(async () =>
            {
                _allPins = await MainActivity.dayTomatoClient.GetPins();
                _autoCompleteAdapter.AddAll(_allPins.Select(p => p.Name).ToList());
                // Updates autocomplete data
                _autoCompleteAdapter.NotifyDataSetChanged();
            });

            return view;
        }

		private void SetListeners()
		{
			_autocompleteTextView.ItemClick += OnItemClick;
			_createPin.Click += OnCreatePinClick;
		}

		private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{ 
			Pin pin = _allPins[e.Position];
			if (pin != null)
			{
				_addedPins.Add(pin);
				_addedPinsIds.Add(pin.Id);
				Toast.MakeText(Activity, "Added Pin", ToastLength.Short).Show();
				_listPins.Text += _autocompleteTextView.Text + "\n";
				_autocompleteTextView.Text = "";
			}
			else
			{
				Toast.MakeText(Activity, "Could not find pin", ToastLength.Short).Show();
			}
		}

		private void OnCreatePinClick(object sender, System.EventArgs e)
		{
			Intent intent = new Intent(Activity, typeof(MapActivity));
			intent.PutExtra("CREATE_PLACE_REQUEST", true);
			Activity.StartActivityForResult(intent, Constants.CREATE_PLACE_REQUEST);
		}

		public async void AddPin(string pinId)
		{
			Pin pin = await MainActivity.dayTomatoClient.GetPin(pinId);
			_autoCompleteAdapter.Add(pin.Name);
			_autoCompleteAdapter.NotifyDataSetChanged();
			_addedPins.Add(pin);
			_addedPinsIds.Add(pinId);
			Toast.MakeText(Activity, "Added Pin", ToastLength.Short).Show();
			_listPins.Text += pin.Name + "\n";
		}

		public List<Pin> FinalizePins()
        {
			// Store and output data to the parent fragment
			return _addedPins;
        }
    }
}