
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using DayTomato.Models;
using Android.Support.Design.Widget;

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
			var toolbar = FindViewById<Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = "Local Places";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			_createPin = FindViewById<FloatingActionButton>(Resource.Id.place_create_pin);

			InitInstances();
			SetListeners();
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		private async void InitInstances()
		{
			ProgressDialog pd = new ProgressDialog(this);
			pd.Show();
			pd.SetMessage("Loading...");
			_pins = await MainActivity.dayTomatoClient.GetHotPins();
			_recyclerView = FindViewById<RecyclerView>(Resource.Id.places_recycler_view);
			_layoutManager = new LinearLayoutManager(this);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewPinAdapter(_pins, this);
			_recyclerView.SetAdapter(_adapter);
			pd.Hide();
		}

		private void SetListeners()
		{
			_createPin.Click += OnCreatePin;
		}

		private async void OnCreatePin(object sender, System.EventArgs e)
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
