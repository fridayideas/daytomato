using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Android.Util;
using DayTomato.Models;
using Java.Lang;
using System.Collections.Generic;
using Android.Support.V7.Widget;

namespace DayTomato.Droid.Fragments
{
	class TripFragment : Fragment
	{
		private readonly static string TAG = "TRIP_FRAGMENT";

		private List<Trip> _suggestions;

		private LatLng _currentLocation;
		private TextView _userLocation;

		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private TripSuggestionAdapter _adapter;

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
	}
}