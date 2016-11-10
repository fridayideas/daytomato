
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
	[Activity(Label = "CreateTripActivity")]
	public class CreateTripActivity : AppCompatActivity
	{

		private Button _nextButton;                                   // Move on to adding pins to the trip button
		private Button _cancelButton;                                    // Cancel create trip button
		private FrameLayout _frame;
		private CreateTrip _trip;
		private enum Step { DETAILS, PINS };
		private Step _step = Step.DETAILS;
		private CreateTripDetailsFragment _detailsFragment;
		private AddPinsFragment _pinsFragment;
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
			_cancelButton = (Button)FindViewById(Resource.Id.create_trip_cancel_button);
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

			_cancelButton.Click += (sender, e) =>
			{
				Finish();
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
		}

		private void SetTripPins()
		{
			List<Pin> pins = _pinsFragment.FinalizePins();
			if (pins != null)
			{
				FragmentManager
				   .BeginTransaction()
				   .Remove(_pinsFragment)
				   .Commit();

				// Trip filled with pins
				_trip.Pins = pins.Select(p => p.Id).ToList();

				// Now trip can be created
				CreateTrip();
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
				SetResult(Result.Ok, returnIntent);
			}
			else
			{
				SetResult(Result.Canceled, returnIntent);
			}

			Finish();
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
