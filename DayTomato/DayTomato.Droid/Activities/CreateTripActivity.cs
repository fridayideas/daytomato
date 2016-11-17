
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	[Activity(Label = "CreateTripActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class CreateTripActivity : AppCompatActivity
	{

		private Button _nextButton;                                   // Move on to adding pins to the trip button
		private FrameLayout _frame;

		private CreateTripDetailsFragment _detailsFragment;
		private AddPinsFragment _pinsFragment;

		private CreateTrip _trip;
		List<Pin> _pins;
		private enum Step { DETAILS, PINS };
		private Step _step = Step.DETAILS;
		private bool _tripCreated;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.create_trip_activity);

			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = "Create A Trip";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			_nextButton = (Button)FindViewById(Resource.Id.create_trip_next_button);
            _frame = (FrameLayout)FindViewById(Resource.Id.create_trip_frame);

			_detailsFragment = CreateTripDetailsFragment.NewInstance();
			_pinsFragment = AddPinsFragment.NewInstance();

			FragmentManager
				.BeginTransaction()
				.Replace(_frame.Id, _detailsFragment, "CreateTripDetailsFragment")
				.Commit();

			SetListeners();
		}

		private void SetListeners()
		{
			_nextButton.Click += (sender, e) =>
			{
				if (_step == Step.DETAILS)
				{
					SetTripDetails();
				}
				else if (_step == Step.PINS)
				{
					SetTripPins();
				}
			};
        }

		private void SetTripDetails()
		{
			CreateTrip trip = _detailsFragment.FinalizeTripDetails();
			if (trip != null)
			{
				_step = Step.PINS;
				FragmentManager
					.BeginTransaction()
					.Replace(_frame.Id, _pinsFragment, "AddPinsFragment")
					.Commit();

				// Trip filled with details
				_trip = trip;
			}
			else
			{
				Toast.MakeText(this, "Please enter the trip details", ToastLength.Long).Show();
			}
		}

		private void SetTripPins()
		{
			_pins = _pinsFragment.FinalizePins();
			if (_pins != null && _pins.Count > 0)
			{
				FragmentManager
				   .BeginTransaction()
				   .Remove(_pinsFragment)
				   .Commit();

				// Trip filled with pins
				_trip.Pins = _pins.Select(p => p.Id).ToList();

				// Now trip can be created
				CreateTrip();
			}
			else
			{
				Toast.MakeText(this, "Please enter at least one place", ToastLength.Long).Show();
			}
		}

		private async void CreateTrip()
		{
			MainActivity.UpdateAccount(MainActivity.GetAccount().Id, 1, 1);
			_trip.Id = await MainActivity.dayTomatoClient.CreateTrip(_trip);
			_tripCreated = _trip.Id != null;

			Intent returnIntent = new Intent();

			if (_tripCreated)
			{
				//TODO: Send request to server to add to my trips
				MainActivity.AddToMyTrips(new Trip(_trip, _pins));
				SetResult(Result.Ok, returnIntent);
			}
			else
			{
				SetResult(Result.Canceled, returnIntent);
			}

			Finish();
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
					_pinsFragment.AddPin(pinId);
				}
			}
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
