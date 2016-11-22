
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using Segment;
using Segment.Model;

namespace DayTomato.Droid
{
	[Activity(Label = "TripControlPanel", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class TripControlPanel : AppCompatActivity
	{

		private Button _viewTrips;
		private Button _createTrip;
		private Button _viewPlaces;
		private Button _viewMap;

        private Account _account;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.trip_control_panel);

			string title = Intent.GetStringExtra("TRIP_LOCALITY");
			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = title;
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			_viewTrips = FindViewById<Button>(Resource.Id.trip_control_panel_view_trips);
			_createTrip = FindViewById<Button>(Resource.Id.trip_control_panel_create_trip);
			_viewPlaces = FindViewById<Button>(Resource.Id.trip_control_panel_view_local_places);
			_viewMap = FindViewById<Button>(Resource.Id.trip_control_panel_view_map);

            _account = MainActivity.GetAccount();

			SetListeners();
		}

		private void SetListeners()
		{
			_viewTrips.Click += SetViewTripsOnClick;
			_createTrip.Click += SetCreateTripOnClick;
			_viewPlaces.Click += SetViewPlacesOnClick;
			_viewMap.Click += SetViewMapOnClick;
		}

		private void SetViewTripsOnClick(object sender, System.EventArgs e)
		{
            Analytics.Client.Screen(_account.Id, "Local trips view", new Properties()
            {
                { "Trips", "View" }
            });
            Intent intent = new Intent(this, typeof(TripsActivity));
			StartActivityForResult(intent, Constants.ADD_TRIP_REQUEST);
		}

		private void SetCreateTripOnClick(object sender, System.EventArgs e)
		{
            Analytics.Client.Screen(_account.Id, "Create trip view", new Properties()
            {
                { "Create Trip", "View" }
            }); ;
            Intent intent = new Intent(this, typeof(CreateTripActivity));
			StartActivityForResult(intent, Constants.CREATE_TRIP_REQUEST);
		}

		private void SetViewPlacesOnClick(object sender, System.EventArgs e)
		{
            Analytics.Client.Screen(_account.Id, "Local places view", new Properties()
            {
                { "Pins", "View" }
            }); ;
			Intent intent = new Intent(this, typeof(PlacesActivity));
			StartActivity(intent);
		}

		private void SetViewMapOnClick(object sender, System.EventArgs e)
		{
            Analytics.Client.Screen(_account.Id, "Map view", new Properties()
            {
                { "Map", "View" }
            }); ;
            Intent intent = new Intent(this, typeof(MapActivity));
			StartActivity(intent);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == Constants.CREATE_TRIP_REQUEST)
			{
				if (resultCode == Result.Ok)
				{
					Toast.MakeText(this, "Your trip was created", ToastLength.Long).Show();
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
