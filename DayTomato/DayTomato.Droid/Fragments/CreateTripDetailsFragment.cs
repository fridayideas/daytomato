
using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Text.RegularExpressions;
using Android.App;

namespace DayTomato.Droid
{
    public class CreateTripDetailsFragment : DialogFragment
    {

        public event EventHandler<CreateTripDetailsEventArgs> CreateTripDetailsFinished;      // Event handler when user presses create
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

			_cost.TextChanged += (sender, e) => 
			{
				if (!IsValidInput(e.Text.ToString()))
				{
					_cost.Error = "Please enter a valid cost";
				}
			};

            return view;
        }

        public bool FinalizeTripDetails()
		{
			if (!IsValidInput(_cost.Text)) return false;
            // Store and output data to the parent fragment
			if (CreateTripDetailsFinished != null)
            {
                CreateTripDetailsFinished(this, new CreateTripDetailsEventArgs
                {
                    Name = _name.Text,
                    Type = _type.Text,
                    Description = _description.Text,
                    Cost = Convert.ToDouble(_cost.Text),
                    CreateDate = DateTime.Today,
                });

                MainActivity.UpdateAccount(MainActivity.GetAccount().Id, 1, 1);
				return true;
            }
			return false;
        }

		private bool IsValidInput(string cost)
		{
			Regex regex = new Regex(@"[0-9]+");
			return regex.IsMatch(cost);
		}
    }

    public class CreateTripDetailsEventArgs
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public DateTime CreateDate { get; set; }
    }
}