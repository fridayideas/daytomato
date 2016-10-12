using System.Collections.Generic;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using DayTomato.Services;

namespace DayTomato.Droid.Fragments
{
    class HomeFragment : Fragment
    {
		private List<Feed> _feed;

		private TextView _username;
		private ImageView _profilePicture;
		private static TextView _pinCount;
		private static TextView _seedCount;

		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private HomeFeedAdapter _adapter;

		private static Account _account;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.home_fragment, container, false);
			_username = (TextView)view.FindViewById(Resource.Id.home_user_name);
			_profilePicture = (ImageView)view.FindViewById(Resource.Id.home_profile_picture);
			_pinCount = (TextView)view.FindViewById(Resource.Id.home_pin_count);
			_seedCount = (TextView)view.FindViewById(Resource.Id.home_seed_count);

			InitInstances();

			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.home_recycler_view);
			_layoutManager = new LinearLayoutManager(Context);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new HomeFeedAdapter(_feed, this.Activity);
			_recyclerView.SetAdapter(_adapter);

            return view;
        }

		public override void OnResume()
		{
			base.OnResume();
		}

		public static void UpdateHomePage()
		{
			while (_account == null)
			{
				_account = MainActivity.GetAccount();
			}
			_pinCount.Text = _account.Pins.ToString();
			_seedCount.Text = _account.Seeds.ToString();
		}

		private async void InitInstances()
		{
			while (_account == null)
			{
				_account = MainActivity.GetAccount();
			}
			_username.Text = _account.Username;
			_pinCount.Text = _account.Pins.ToString();
			_seedCount.Text = _account.Seeds.ToString();

			// Get feed from server
			_feed = new List<Feed>();
			_feed.Add(new Feed
			{
				Notification = "User 1 liked your pin",
				Type = Feed.FEED_NOTIFICATION
			});
			_feed.Add(new Feed
			{
				Notification = "User 2 disliked your pin",
				Type = Feed.FEED_NOTIFICATION
			});

			var pins = await MainActivity.dayTomatoClient.GetHotPins();
			foreach (var p in pins)
			{
				_feed.Add(new Feed
				{
					Pin = p,
					Type = Feed.FEED_PIN
				});
			}
			_adapter.NotifyDataSetChanged();
		}
    }
}