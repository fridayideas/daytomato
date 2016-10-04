using System.Collections.Generic;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid.Fragments
{
    class HomeFragment : Fragment
    {
		private List<string> _feed;

		private TextView _username;
		private ImageView _profilePicture;
		private TextView _pinCount;
		private TextView _seedCount;

		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private HomeFeedAdapter _adapter;

		private Account _account;

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
			_pinCount = (TextView)view.FindViewById(Resource.Id.home_seed_count);
			_seedCount = (TextView)view.FindViewById(Resource.Id.home_pin_count);

			InitInstances();

			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.home_recycler_view);
			_layoutManager = new LinearLayoutManager(Context);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new HomeFeedAdapter(_feed);
			_recyclerView.SetAdapter(_adapter);

            return view;
        }

		public override void OnResume()
		{
			base.OnResume();
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

			//TODO: Get feed from server
			while (_feed == null)
			{
				_feed = new List<string>();
				_feed.Add("User 1 liked your pin");
				_feed.Add("User 2 disliked your pin");
			}
		}
    }
}