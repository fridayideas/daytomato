using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Runtime;
using Android.Support.V4.View;
using DayTomato.Droid.Fragments;
using Java.Lang;
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
using Plugin.Media;

namespace DayTomato.Droid
{
    [Activity(Icon = "@drawable/icon")]
    public class MainActivity : FragmentActivity, ViewPager.IOnPageChangeListener
	{
		private static readonly string TAG = "MAIN_ACTIVITY";

        private TabLayout _tabLayout;
		private ViewPager _viewPager;
		private LatLng _currentLocation;
		private static Account _account;
	    private string _idToken; //IdToken provided by auth0. It is used to authenticate the current user on the server.

	    internal IGeolocator Locator { get; set; }

        public static DayTomatoClient dayTomatoClient;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
            toolbar.SetTitle(Resource.String.application_name);

            //Set ID token provided by LoginActivity
            _idToken = Intent.GetStringExtra("AuthIdToken");

            // REST API Client
            dayTomatoClient = new DayTomatoClient(_idToken);

            // Get location
            Locator = CrossGeolocator.Current;
            await Locator.StartListeningAsync(1, 0);
            Locator.PositionChanged += (sender, args) =>
            {
                var pos = args.Position;
                _currentLocation = new LatLng(pos.Latitude, pos.Longitude);
            };
            //_currentLocation = await GetUserLocation();
            _currentLocation = new LatLng(0, 0);

			// Get user account
			_account = await GetUserAccount();

            // Tabs
            _tabLayout = FindViewById<TabLayout>(Resource.Id.main_sliding_tabs);
            InitTabLayout();
        }

        /// <summary>
        /// Call GetAccount in core to obtain 
        /// </summary>
        /// <returns></returns>
		public async Task<Account> GetUserAccount()
		{
            _account = await dayTomatoClient.GetAccount();

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

		public LatLng GetLocation()
		{
			return _currentLocation;
		}

		public static void UpdateAccount(string accountId, double seeds, int pins)
		{
			_account.Pins += pins;
			_account.Seeds += seeds;
		}

		/*
		 * TABS SECTION
		 */
		private void InitTabLayout()
        {
            _tabLayout.SetTabTextColors(Android.Graphics.Color.White, Android.Graphics.Color.White);
            //Fragment array
            var fragments = new Android.Support.V4.App.Fragment[]
            {
                new HomeFragment(),
                new MapFragment(),
				new TripFragment()
            };
            //Tab title array
            var titles = CharSequence.ArrayFromStringArray(new[] {
                "Home",
                "Map",
				"Trips"
            });

            _viewPager = FindViewById<ViewPager>(Resource.Id.main_viewpager);
            //viewpager holding fragment array and tab title text
            _viewPager.Adapter = new TabsFragmentPagerAdapter(SupportFragmentManager, fragments, titles);
			_viewPager.AddOnPageChangeListener(this);
            // Give the TabLayout the ViewPager 
            _tabLayout.SetupWithViewPager(_viewPager);
			//_tabLayout.SetOnTabSelectedListener(this);
            SetIcons();
        }

        void SetIcons()
        {
            _tabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_home_white_24dp);
            _tabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_place_white_24dp);
			_tabLayout.GetTabAt(2).SetIcon(Resource.Drawable.ic_directions_walk_white_24px);
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            dialog.Dismiss();
        }

		public void OnPageScrollStateChanged(int state)
		{
			return;
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			return;
		}

		public void OnPageSelected(int position)
		{
			switch (position)
			{
				// Home Fragment
				case 0:
					HomeFragment.UpdateHomePage();
					break;
				// Map Fragment
				case 1:
					break;
				// Trip Fragment
				case 2:
					break;
			}
		}

	    public override void OnBackPressed(){}//Do nothing when back button pressed

	    public class TabsFragmentPagerAdapter : FragmentPagerAdapter
        {
            private readonly Android.Support.V4.App.Fragment[] fragments;

            private readonly ICharSequence[] titles;

            public TabsFragmentPagerAdapter(Android.Support.V4.App.FragmentManager fm,
                                            Android.Support.V4.App.Fragment[] fragments, 
                                            ICharSequence[] titles) : base(fm)
            {
                this.fragments = fragments;
                this.titles = titles;
            }
            public override int Count
            {
                get
                {
                    return fragments.Length;
                }
            }

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                return fragments[position];
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return titles[position];
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







