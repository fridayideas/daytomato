
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using DayTomato.Droid.Adapters;
using DayTomato.Models;
using DayTomato.Services;
using Newtonsoft.Json;

namespace DayTomato.Droid
{
	[Activity(Label = "TripsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class TripsActivity : AppCompatActivity, DeleteTripListener
	{
		
		private const string TAG = "TRIP_ACTIVITY";

		private List<Trip> _trips;
		private CreateTrip _trip;

		// Button to create new trip
		private FloatingActionButton _createTripButton;
		private TextView _userLocation;
		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewTripAdapter _adapter;

		public static DayTomatoClient dayTomatoClient;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.trip_activity);
			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = "Trips";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			dayTomatoClient = MainActivity.dayTomatoClient;

			_createTripButton = (FloatingActionButton)FindViewById(Resource.Id.trip_create_fab);
			_userLocation = (TextView)FindViewById(Resource.Id.trip_current_location);
			_recyclerView = FindViewById<RecyclerView>(Resource.Id.trip_recycler_view);

			InitInstances();
			SetListeners();
		}

		private async void InitInstances()
		{
			_trips = await MainActivity.dayTomatoClient.GetTrips();

			_layoutManager = new LinearLayoutManager(this);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewTripAdapter(_trips, this);
			_adapter.HandleClick += OnHandleClick;
			_recyclerView.SetAdapter(_adapter);

			var currentLoc = MainActivity.GetLocation();

			// Reverse geocode coordinates
			var geo = new Geocoder(this);
			var addresses = await geo.GetFromLocationAsync(currentLoc.Latitude, currentLoc.Longitude, 1);

			string address = "Location Unknown";
			if (addresses.Count > 0)
			{
				try
				{
					address = addresses[0].GetAddressLine(0);
				}
				catch (Exception ex)
				{
					Log.Error(TAG, ex.ToString());
				}
			}
			_userLocation.Text = "Your location: " + address;

        }

		private async void RefreshTrips()
		{
			_trips = await MainActivity.dayTomatoClient.GetTrips();
			_adapter = new ViewTripAdapter(_trips, this);
			_adapter.HandleClick += OnHandleClick;
			_recyclerView.SetAdapter(_adapter);
		}

		public async void OnDeleteTrip(Trip trip)
		{
			_trips.Remove(trip);
			await dayTomatoClient.DeleteTrip(trip.Id);
		}

		private void OnHandleClick(object sender, int position)
		{
			var tripData = JsonConvert.SerializeObject(_trips[position]);

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
			bundle.PutString("VIEW_TRIP_TITLE", _trips[position].Name);
			bundle.PutString("VIEW_TRIP_DATA", tripData);

			var viewTripDialogFragment = ViewTripDialogFragment.NewInstance(bundle);

			//Add fragment
			viewTripDialogFragment.Show(fm, "ViewTripDialog");
		}

		private void SetListeners()
		{
			_createTripButton.Click += (sender, args) =>
			{
				Intent intent = new Intent(this, typeof(CreateTripActivity));
				StartActivityForResult(intent, Constants.CREATE_TRIP_REQUEST);
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == Constants.CREATE_TRIP_REQUEST)
			{
				if (resultCode == Result.Ok)
				{
					Toast.MakeText(this, "Your trip was created", ToastLength.Long).Show();
					RefreshTrips();
				}
			}
		}

		public override void OnBackPressed()
		{
			base.OnBackPressed();
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
				default:
					return base.OnOptionsItemSelected(item);
			}
		}
	}
}
