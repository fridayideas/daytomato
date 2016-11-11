﻿
using System;
using Android.App;
using Android.OS;
using Android.Content;
using DayTomato.Services;
using Android.Gms.Maps.Model;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Android.Util;
using DayTomato.Models;
using Java.IO;
using Android.Graphics;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator.Abstractions;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Locations;
using Android.Support.V7.App;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DayTomato.Droid
{
    [Activity(Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : AppCompatActivity
	{

		private static readonly string TAG = "MAIN_ACTIVITY";

		private AutoCompleteTextView _cityAutocomplete;
		private Button _cityControlPanelButton;

		private DrawerLayout _drawer;
		private NavigationView _navigation;

		private static List<Trip> _myTrips;
		private static bool _refresh = false;
		private ViewTripAdapter _adapter;
		private RecyclerView _recyclerView;
		private LinearLayoutManager _layoutManager;

		private static LatLng _currentLocation;
		private string _locality;
		private static Account _account;
	    private string _idToken; //IdToken provided by auth0. It is used to authenticate the current user on the server.

	    internal IGeolocator Locator { get; set; }

        public static DayTomatoClient dayTomatoClient;
		public static ImgurClient imgurClient;
		public static GoogleClient googleClient;
	    private string[] _citySearchPredictions;
	    private ArrayAdapter _citySearchAdapter;

	    protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

			Window.SetSoftInputMode(Android.Views.SoftInput.StateAlwaysHidden);

			// Autocomplete text view
			_cityAutocomplete = FindViewById<AutoCompleteTextView>(Resource.Id.main_city_autocomplete);
			// Go to control panel button
			_cityControlPanelButton = FindViewById<Button>(Resource.Id.main_start_control_panel_button);
			// My Trips recyclerview
			_recyclerView = FindViewById<RecyclerView>(Resource.Id.main_my_trips_recycler_view);
			// Main drawer
			_drawer = FindViewById<DrawerLayout>(Resource.Id.main_drawer);
			// Main drawer navigation
			_navigation = FindViewById<NavigationView>(Resource.Id.main_nav_view);

			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			toolbar.SetTitle(Resource.String.application_name);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_24px);

			// Create ActionBarDrawerToggle button and add it to the toolbar
			var drawerToggle = new ActionBarDrawerToggle(this, _drawer, toolbar, Resource.String.drawer_open, Resource.String.drawer_close);
			_drawer.AddDrawerListener(drawerToggle);
			drawerToggle.SyncState();

            //Set ID token provided by LoginActivity
            _idToken = Intent.GetStringExtra("AuthIdToken");

            // REST API Client
            dayTomatoClient = new DayTomatoClient(_idToken);
			imgurClient = new ImgurClient();
			googleClient = new GoogleClient();

            // Get location
			_currentLocation = new LatLng(0, 0);
            Locator = CrossGeolocator.Current;
			await Locator.StartListeningAsync(1, 0);
            Locator.PositionChanged += (sender, args) =>
            {
                var pos = args.Position;
                _currentLocation = new LatLng(pos.Latitude, pos.Longitude);
            };

			// Get user account
			_account = await GetUserAccount();

			SetListeners();
			SetInstances();
        }

		protected override void OnResume()
		{
			base.OnResume();
			RefreshMyTrips();
		}

		private void SetListeners()
		{
			_navigation.NavigationItemSelected += SetNavigationOnClick;
			_cityControlPanelButton.Click += SetCityControlPanelButtonOnClick;

            _cityAutocomplete.TextChanged += async (sender, e) =>
            {
                try
                {
                    _citySearchPredictions = await googleClient.PredictCities(e.Text.ToString());
                    _citySearchAdapter = new ArrayAdapter(this,
                                                         Android.Resource.Layout.SimpleDropDownItem1Line,
                                                         _citySearchPredictions);
                    _cityAutocomplete.Adapter = _citySearchAdapter;
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, ex.Message);
                }
            };

            //TODO: Add further functionality that depends on user selection 
            //_cityAutocomplete.ItemClick += async (Sender, e) =>
            //{
            //    try
            //    {
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(TAG, ex.Message);
            //    }
            //};
        }

		private void SetInstances()
		{
			SetButtonLocation();
			GetMyTrips();
		}

		private async void SetButtonLocation()
		{
			_currentLocation = await GetUserLocation();

			var geo = new Geocoder(this);
			var addresses = await geo.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 1);

			_locality = "Current Location";
			if (addresses.Count > 0)
			{
				_locality = addresses[0].Locality;
			}

			_cityControlPanelButton.Text = "Explore " + _locality;
		}

		private void GetMyTrips()
		{
			//TODO: Get my trips from server
			_myTrips = new List<Trip>();

			_layoutManager = new LinearLayoutManager(this);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewTripAdapter(_myTrips, this);
			_adapter.HandleClick += OnHandleClick;
			_recyclerView.SetAdapter(_adapter);
		}

		public static void AddToMyTrips(Trip trip)
		{
			if (!_myTrips.Contains(trip))
			{
				_myTrips.Add(trip);
				_refresh = true;
			}
		}

		private void RefreshMyTrips()
		{
			if (_refresh && _adapter != null)
			{
				_adapter.NotifyDataSetChanged();
				_refresh = false;
			}
		}

		private void OnHandleClick(object sender, int position)
		{
			var tripData = JsonConvert.SerializeObject(_myTrips[position]);

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
			bundle.PutString("VIEW_TRIP_TITLE", _myTrips[position].Name);
			bundle.PutString("VIEW_TRIP_DATA", tripData);

			var viewTripDialogFragment = ViewTripDialogFragment.NewInstance(bundle);

			//Add fragment
			viewTripDialogFragment.Show(fm, "ViewTripDialog");
		}

		private void SetCityControlPanelButtonOnClick(object sender, System.EventArgs e)
		{
			Intent intent = new Intent(this, typeof(TripControlPanel));
			intent.PutExtra("TRIP_LOCALITY", _locality);
			StartActivityForResult(intent, Constants.TRIP_CONTROL_PANEL);
		}

		private void SetNavigationOnClick(object sender, NavigationView.NavigationItemSelectedEventArgs e)
		{
			switch (e.MenuItem.ItemId)
			{
				case (Resource.Id.nav_home):
					// React on 'nav_home' selection
					break;
				case (Resource.Id.nav_map):
					//
					break;
				case (Resource.Id.nav_about):
					break;
				case (Resource.Id.nav_feedBack):
					break;
			}
			// Close drawer
			_drawer.CloseDrawers();
		}

        /// <summary>
        /// Call GetAccount in core to obtain 
        /// </summary>
        /// <returns></returns>
		public async Task<Account> GetUserAccount()
		{
            _account = await dayTomatoClient.GetAccount();
            
            //Set name in Home tab to first name
		    string googleAccInfoJSON = Intent.GetStringExtra("AuthUserJSON");
            JToken googleAccInfoObj = JObject.Parse(googleAccInfoJSON);

            _account.Username = (string)googleAccInfoObj["given_name"];
		    string imageurl = (string)googleAccInfoObj["picture"];
            _account.ProfilePicture = await dayTomatoClient.GetImageBitmapFromUrlAsync(imageurl);

            return _account;
		}

		public async Task<LatLng> GetUserLocation()
		{
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;
			try
			{
				var position = await locator.GetPositionAsync(timeoutMilliseconds: 20000);
				_currentLocation = new LatLng(position.Latitude, position.Longitude);
			}
			catch (TaskCanceledException tc)
			{
				Log.Error(TAG, tc.Message);
				_currentLocation = new LatLng(0.0, 0.0);
			}
			catch (Exception ex)
			{
				Log.Error(TAG, ex.ToString());
				_currentLocation = new LatLng(0.0, 0.0);
			}

			return _currentLocation;
		}

		public static Account GetAccount()
		{
			return _account;
		}

		public static LatLng GetLocation()
		{
			return _currentLocation;
		}

		public static async void UpdateAccount(string accountId, double seeds, int pins)
		{
			_account.Pins += pins;
			_account.Seeds += seeds;
			if (seeds > 0)
			{
				await dayTomatoClient.UpdateAccountSeeds(accountId, seeds);
			}
			if (pins > 0)
			{
				await dayTomatoClient.UpdateAccountPins(accountId, pins);
			}
		}
	}

	public static class Picture
	{
		public static File File { get; set; }
		public static File Dir { get; set; }
		public static Bitmap Bitmap { get; set; }
	}
}







