
using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Text.RegularExpressions;
using Android.App;
using DayTomato.Models;

namespace DayTomato.Droid
{
    public class CreateTripDetailsFragment : DialogFragment
    {

        private EditText _name;                                          // Name user will put
        private EditText _type;                                          // Type of trip
        private EditText _description;                                   // Description user will put
        private EditText _cost;                                          // Amount user spent

        public static CreateTripDetailsFragment NewInstance()
        {
            CreateTripDetailsFragment fragment = new CreateTripDetailsFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.create_trip_details_fragment, container, false);
            _name = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_name);
            _type = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_type);
            _description = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_text_description);
            _cost = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_cost);

            return view;
        }

        public CreateTrip FinalizeTripDetails()
		{
			if (!IsValidInput())
			{
				return null;
			}
			CreateTrip trip = new CreateTrip();
			trip.Name = _name.Text;
			trip.Type = _type.Text;
			trip.Description = _description.Text;
			trip.Cost = Convert.ToDouble(_cost.Text);
			trip.CreateDate = DateTime.Today;

			return trip;
        }

		private bool IsValidInput()
		{
			bool cost = true;
			bool name = true;

			// Cost validation
			Regex regex = new Regex(@"[0-9]+");
			if (!regex.IsMatch(_cost.Text))
			{
				_cost.Error = "Please enter a valid cost";
				cost = false;
			}
			// Name validation
			if (_name.Text == null || _name.Text == "")
			{
				_name.Error = "Please enter a valid name";
				name = false;
			}

			return cost && name;
		}
    }
}