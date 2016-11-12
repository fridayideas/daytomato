
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using Android.Support.Design.Widget;
using DayTomato.Droid.Adapters;

namespace DayTomato.Droid
{
	[Activity(Label = "PlacesActivity")]
	public class PlacesActivity : AppCompatActivity
	{
		private List<Pin> _pins;
        private FloatingActionButton _createPin;
        private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewPinAdapter _adapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.places_activity);
			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = "Local Places";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			InitInstances();

			_createPin = FindViewById<FloatingActionButton>(Resource.Id.place_create_pin);
			_recyclerView = FindViewById<RecyclerView>(Resource.Id.places_recycler_view);
			_layoutManager = new LinearLayoutManager(this);
			_recyclerView.SetLayoutManager(_layoutManager);

			SetListeners();
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		private async void RefreshPlaces()
		{
			_pins = await MainActivity.dayTomatoClient.GetHotPins();
			_adapter.NotifyDataSetChanged();
		}

		private async void AddPlace(string pinId)
		{
			Pin pin = await MainActivity.dayTomatoClient.GetPin(pinId);
			_pins.Add(pin);
			_adapter.NotifyItemInserted(_pins.Count);
			_recyclerView.SmoothScrollToPosition(_adapter.ItemCount);
		}

		private async void InitInstances()
		{
			ProgressDialog pd = new ProgressDialog(this);
			pd.Show();
			pd.SetMessage("Loading...");

			// Get feed from server
			_pins = await MainActivity.dayTomatoClient.GetHotPins();
			_adapter = new ViewPinAdapter(_pins, this);
			_recyclerView.SetAdapter(_adapter);

			pd.Hide();
		}

		private void SetListeners()
		{
			_createPin.Click += OnCreatePin;
		}

		private void OnCreatePin(object sender, System.EventArgs e)
		{
			Intent intent = new Intent(this, typeof(MapActivity));
			intent.PutExtra("CREATE_PLACE_REQUEST", true);
			StartActivityForResult(intent, Constants.CREATE_PLACE_REQUEST);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode == Constants.CREATE_PLACE_REQUEST)
			{
				if (resultCode == Result.Ok)
				{
					Toast.MakeText(this, "Your place was created", ToastLength.Long).Show();
					string pinId = data.GetStringExtra("CREATE_PLACE_RESULT");
					AddPlace(pinId);
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
