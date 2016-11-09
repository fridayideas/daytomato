
using System.Collections.Generic;
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
			_detailsFragment.CreateTripDetailsFinished += OnDetailsFinished;
			_pinsFragment = AddPinsFragment.NewInstance();
			_pinsFragment.AddPinsFinished += OnAddPins;

			FragmentManager
				.BeginTransaction()
				.Replace(_frame.Id, _detailsFragment, "CreateTripDetailsFragment")
				.Commit();

			SetListeners();
		}

		private void OnDetailsFinished(object sender, CreateTripDetailsEventArgs e)
		{
			var account = MainActivity.GetAccount();
			_trip = new CreateTrip();

			_trip.Type = e.Type;
			_trip.Name = e.Name;
			_trip.Description = e.Description;
			_trip.Cost = e.Cost;
			_trip.LinkedAccount = account.Username;
			_trip.CreateDate = e.CreateDate;
			_trip.LikedBy = new List<string>();
			_trip.DislikedBy = new List<string>();
			_trip.Comments = new List<Comment>();
			_trip.Likes = 0;
			_trip.Rating = 0;
		}

		private async void OnAddPins(object sender, AddPinsEventArgs e)
		{
			_trip.Pins = e.PinsIds;
			_trip.Id = await MainActivity.dayTomatoClient.CreateTrip(_trip);
			_tripCreated = _trip.Id != null ? true : false;
			CreateTrip();
		}

		private void SetListeners()
		{
			_nextButton.Click += (sender, e) =>
			{
				if (_step == Step.DETAILS)
				{
					if (_detailsFragment.FinalizeTripDetails())
					{
						_step = Step.PINS;
						FragmentManager
							.BeginTransaction()
							.Replace(_frame.Id, _pinsFragment, "AddPinsFragment")
							.Commit();
					}
				}
				else if (_step == Step.PINS)
				{
					if (_pinsFragment.FinalizePins())
					{
						FragmentManager
						   .BeginTransaction()
						   .Remove(_pinsFragment)
						   .Commit();
						
					}
				}
			};

			_cancelButton.Click += (sender, e) =>
			{
				Finish();
			};
		}

		private void CreateTrip()
		{
			Toast.MakeText(this, "Your trip was created", ToastLength.Short);
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
					CreateTrip();
					return true;

				default:
					return base.OnOptionsItemSelected(item);
			}
		}
	}
}
