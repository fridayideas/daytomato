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
		private const string TAG = "TRIP_FRAGMENT";

        // Button to create new trip
        private FloatingActionButton _createTripButton;

        private List<Trip> _suggestions;

		private TextView _userLocation;

		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewTripAdapter _adapter;

	    private MainActivity _activity;

        private CreateTrip trip = new CreateTrip();

        private bool _lock;

        public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.trip_fragment, container, false);
            _lock = false;
			_createTripButton = (FloatingActionButton)view.FindViewById(Resource.Id.trip_create_fab);
            _userLocation = (TextView)view.FindViewById(Resource.Id.trip_current_location);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.trip_recycler_view);
		    _activity = (MainActivity) Activity;

            InitInstances();
            SetListeners();

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
			_adapter = new ViewTripAdapter(_suggestions, Activity);
			_adapter.HandleClick += OnHandleClick;
			_recyclerView.SetAdapter(_adapter);

		    var currentLoc = await _activity.GetUserLocation();

			// Reverse geocode coordinates
			var geo = new Geocoder(Context);
			var addresses = await geo.GetFromLocationAsync(currentLoc.Latitude, currentLoc.Longitude, 1);

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
            _createTripButton.Click += (sender, args) =>
            {
                CreateTripDialog();
            };
        }

        private void CreateTripDialog()
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
            _createTripButton.Visibility = ViewStates.Visible;
            _createTripButton.Enabled = true;

            var createTripDialogFragment = CreateTripDialogFragment.NewInstance();
            createTripDialogFragment.CreateTripDialogClosed += OnCreateTripDialogClosed;

            //Add fragment
            createTripDialogFragment.Show(fm, "CreateTripDialog");
        }

        private void OnViewTripDialogClosed(object sender, ViewTripDialogEventArgs e)
		{
		}

        // Event listener, when the createpin dialog is closed, this will get called
        public async void OnCreateTripDialogClosed(object sender, CreateTripDialogEventArgs e)
        {
            _lock = false;
            var account = MainActivity.GetAccount();

            trip.Type = e.Type;
            trip.Name = e.Name;
            trip.Description = e.Description;
            //trip.Cost = e.Cost;
            trip.LinkedAccount = account.Id;
            trip.CreateDate = e.CreateDate;

            AddPinsDialog();
            //trip.Id = await MainActivity.dayTomatoClient.CreateTrip(trip);
        }

        private void AddPinsDialog()
        {
            _lock = true;
            var fm = FragmentManager;
            var ft = fm.BeginTransaction();

            //Remove fragment else it will crash as it is already added to backstack
            var prev = fm.FindFragmentByTag("AddPinsDialog");
            if (prev != null)
            {
                ft.Remove(prev);
            }

            ft.AddToBackStack(null);

            // Switch button states
            _createTripButton.Visibility = ViewStates.Visible;
            _createTripButton.Enabled = true;
            
            var addPinsDialogFragment = AddPinsDialogFragment.NewInstance();
            addPinsDialogFragment.AddPinsDialogClosed += OnAddPinsDialogClosed;

            //Add fragment
            addPinsDialogFragment.Show(fm, "AddPinsDialog");
        }
        public async void OnAddPinsDialogClosed(object sender, AddPinsDialogEventArgs e)
        {
            _lock = false;
           
            trip.Pins = e.Pins;

            trip.Id = await MainActivity.dayTomatoClient.CreateTrip(trip);

            trip = new CreateTrip();

            this.InitInstances();
        }
    }
}