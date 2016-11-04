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

		private List<Trip> _suggestions;
		CreateTrip _trip;

        // Button to create new trip
        private FloatingActionButton _createTripButton;
		private TextView _userLocation;
		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewTripAdapter _adapter;

        public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.trip_fragment, container, false);
			_createTripButton = (FloatingActionButton)view.FindViewById(Resource.Id.trip_create_fab);
            _userLocation = (TextView)view.FindViewById(Resource.Id.trip_current_location);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.trip_recycler_view);

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

			var currentLoc = MainActivity.GetLocation();

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
        public void OnCreateTripDialogClosed(object sender, CreateTripDialogEventArgs e)
        {
            var account = MainActivity.GetAccount();
			_trip = new CreateTrip();

            _trip.Type = e.Type;
            _trip.Name = e.Name;
            _trip.Description = e.Description;
            _trip.Cost = e.Cost;
            _trip.LinkedAccount = account.Username;
            _trip.CreateDate = e.CreateDate;
			_trip.LikedBy = new List<string>();
			_trip.DislikedBy = new List<string>();
			_trip.Comments = new List<Comment>();
			_trip.Likes = 0;
			_trip.Rating = 0;

            AddPinsDialog();
        }

        private void AddPinsDialog()
        {
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
            _trip.Pins = e.PinsIds;
            _trip.Id = await MainActivity.dayTomatoClient.CreateTrip(_trip);

			_suggestions.Add(new Trip(_trip, e.Pins));
			_adapter.NotifyDataSetChanged();
        }
    }
}