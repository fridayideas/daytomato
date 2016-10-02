using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid.Fragments
{
    class HomeFragment : Fragment
    {
		private TextView _username;
		private ImageView _profilePicture;
		private TextView _pinCount;
		private TextView _seedCount;
		private TextView _userLocation;
		private EditText _budget;
		private Button _makeMyTripButton;

		private LatLng _currentLocation;
		private Account _account;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.home_fragment, container, false);
			_username = (TextView)view.FindViewById(Resource.Id.home_user_name);
			_profilePicture = (ImageView)view.FindViewById(Resource.Id.home_profile_picture);
			_pinCount = (TextView)view.FindViewById(Resource.Id.home_seed_count);
			_seedCount = (TextView)view.FindViewById(Resource.Id.home_pin_count);
			_userLocation = (TextView)view.FindViewById(Resource.Id.home_current_location);
			_budget = (EditText)view.FindViewById(Resource.Id.home_budget_edittext);
			_makeMyTripButton = (Button)view.FindViewById(Resource.Id.home_make_my_trip);

			InitInstances();

            return view;
        }

		public override void OnResume()
		{
			base.OnResume();
		}

		private async void InitInstances()
		{
			_account = await MainActivity.GetUserAccount();
			_username.Text = _account.Username;
			_pinCount.Text = _account.Pins.ToString();
			_seedCount.Text = _account.Seeds.ToString();

			_currentLocation = await MainActivity.GetUserLocation();

			// Reverse geocode coordinates
			var geo = new Geocoder(Context);
			var addresses = await geo.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 1);

			string address = "Unknown Address";
			if (addresses.Count > 0)
			{
				address = addresses[0].GetAddressLine(0);
			}
			_userLocation.Text = address;
		}
    }
}