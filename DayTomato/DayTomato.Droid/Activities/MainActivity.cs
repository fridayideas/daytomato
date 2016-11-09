
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

namespace DayTomato.Droid
{
    [Activity(Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
	{
		
		private static readonly string TAG = "MAIN_ACTIVITY";

		private AutoCompleteTextView _cityAutocomplete;
		private Button _cityControlPanelButton;
		private RecyclerView _myTripsRecyclerView;
		private DrawerLayout _drawer;
		private NavigationView _navigation;

		private static LatLng _currentLocation;
		private string _locality;
		private static Account _account;
	    private string _idToken; //IdToken provided by auth0. It is used to authenticate the current user on the server.

	    internal IGeolocator Locator { get; set; }

        public static DayTomatoClient dayTomatoClient;
		public static ImgurClient imgurClient;
		public static GoogleClient googleClient;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

			this.Window.SetSoftInputMode(Android.Views.SoftInput.StateAlwaysHidden);

			// Autocomplete text view
			_cityAutocomplete = FindViewById<AutoCompleteTextView>(Resource.Id.main_city_autocomplete);
			// Go to control panel button
			_cityControlPanelButton = FindViewById<Button>(Resource.Id.main_start_control_panel_button);
			// My Trips recyclerview
			_myTripsRecyclerView = FindViewById<RecyclerView>(Resource.Id.main_my_trips_recycler_view);
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

		private void SetListeners()
		{
			_navigation.NavigationItemSelected += SetNavigationOnClick;
			_cityControlPanelButton.Click += SetCityControlPanelButtonOnClick;
		}

		private void SetInstances()
		{
			SetButtonLocation();
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

		private void SetCityControlPanelButtonOnClick(object sender, System.EventArgs e)
		{
			Intent intent = new Intent(this, typeof(TripControlPanel));
			intent.PutExtra("TRIP_LOCALITY", _locality);
			StartActivityForResult(intent, Constants.TRIP_CONTROL_PANEL_REQUEST);
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
			catch (System.Exception ex)
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







