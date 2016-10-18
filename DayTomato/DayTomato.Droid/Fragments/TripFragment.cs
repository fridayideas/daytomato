using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Android.Util;
using DayTomato.Models;
using Java.Lang;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Newtonsoft.Json;

namespace DayTomato.Droid.Fragments
{
	class TripFragment : Fragment
	{
		private readonly static string TAG = "TRIP_FRAGMENT";

        // Button to create new pin
        private FloatingActionButton _createTrip;

        private List<Trip> _suggestions;

		private LatLng _currentLocation;
		private TextView _userLocation;

		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private TripSuggestionAdapter _adapter;

        private bool _lock;

        public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.trip_fragment, container, false);

			_userLocation = (TextView)view.FindViewById(Resource.Id.trip_current_location);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.trip_recycler_view);

			InitInstances();

			return view;
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		private async void InitInstances()
		{
			_suggestions = await MainActivity.dayTomatoClient.GetTrips();

			_layoutManager = new LinearLayoutManager(Context);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new TripSuggestionAdapter(_suggestions);
			_adapter.HandleClick += OnHandleClick;
			_recyclerView.SetAdapter(_adapter);

			_currentLocation = await MainActivity.GetUserLocation();

			// Reverse geocode coordinates
			var geo = new Geocoder(Context);
			var addresses = await geo.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 1);

			string address = "Unknown Address";
			if (addresses.Count > 0)
			{
				try
				{
					address = addresses[0].GetAddressLine(0) + "\n";
					address += addresses[0].GetAddressLine(1);
				}
				catch (Exception ex)
				{
					Log.Error(TAG, ex.ToString());
				}
			}
			_userLocation.Text = address;
		}

		private void OnHandleClick(object sender, int position)
		{
			var tripData = JsonConvert.SerializeObject(_suggestions[position]);

			var fm = FragmentManager;
			var ft = fm.BeginTransaction();

			//Remove fragment else it will crash as it is already added to backstack
			var prev = fm.FindFragmentByTag("ViewTripDialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("VIEW_TRIP_TITLE", _suggestions[position].Name);
			bundle.PutString("VIEW_TRIP_DATA", tripData);

			var viewTripDialogFragment = ViewTripDialogFragment.NewInstance(bundle);
			viewTripDialogFragment.ViewTripDialogClosed += OnViewTripDialogClosed;

			//Add fragment
			viewTripDialogFragment.Show(fm, "ViewTripDialog");
		}

        private void SetListeners()
        {
            _createTrip.Click += (sender, args) =>
            {
                // Switch button states
                _createTrip.Visibility = ViewStates.Invisible;
                _createTrip.Enabled = false;
                CreatePinDialog();

            };
        }

        async void CreatePinDialog()
        {
            _lock = true;
            var fm = FragmentManager;
            var ft = fm.BeginTransaction();

            //Remove fragment else it will crash as it is already added to backstack
            var prev = fm.FindFragmentByTag("CreateTripDialog");
            if (prev != null)
            {
                ft.Remove(prev);
            }

            ft.AddToBackStack(null);

            // Switch button states
            _createTrip.Visibility = ViewStates.Visible;
            _createTrip.Enabled = true;

            var createTripDialogFragment = CreateTripDialogFragment.NewInstance();
            createTripDialogFragment.CreateTripDialogClosed += OnCreateTripDialogClosed;

            //Add fragment
            createTripDialogFragment.Show(fm, "CreateTripDialog");
        }

        private void OnViewTripDialogClosed(object sender, ViewTripDialogEventArgs e)
		{
		}

        // Event listener, when the createpin dialog is closed, this will get called
        public async void OnCreateTripDialogClosed(object sender, CreatePinDialogEventArgs e)
        {
            _lock = false;
            var account = MainActivity.GetAccount();
            var trip = new Trip
            {
                Type = e.Type,
                Name = e.Name,
                LinkedAccount = account.Id,
                CreateDate = e.CreateDate
            };

            trip.Id = await MainActivity.dayTomatoClient.CreateTrip(trip);
        }
    }
}