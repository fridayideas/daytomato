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

namespace DayTomato.Droid
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : FragmentActivity
	{
		private static string _account;
        private TabLayout _tabLayout;

        public static DayTomatoClient dayTomatoClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
            toolbar.SetTitle(Resource.String.application_name);

			// For testing only
			_account = "admin";

			// REST API Client
			dayTomatoClient = new DayTomatoClient();

			// Tabs
            _tabLayout = FindViewById<TabLayout>(Resource.Id.main_sliding_tabs);
            InitTabLayout();
        }

		public static string GetAccount()
		{
			return _account;
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
                new PinMapFragment()
            };
            //Tab title array
            var titles = CharSequence.ArrayFromStringArray(new[] {
                "Home",
                "Map"
            });

            var viewPager = FindViewById<ViewPager>(Resource.Id.main_viewpager);
            //viewpager holding fragment array and tab title text
            viewPager.Adapter = new TabsFragmentPagerAdapter(SupportFragmentManager, fragments, titles);

            // Give the TabLayout the ViewPager 
            _tabLayout.SetupWithViewPager(viewPager);
            SetIcons();
        }

        void SetIcons()
        {
            _tabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_home_white_24dp);
            _tabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_place_white_24dp);
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            dialog.Dismiss();
        }

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
}



