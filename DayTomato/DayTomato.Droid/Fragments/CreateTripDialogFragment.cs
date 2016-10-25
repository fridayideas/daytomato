using System;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Util;
using Plugin.Media;
using System.IO;

namespace DayTomato.Droid
{
    public class CreateTripDialogFragment : DialogFragment
    {
        private readonly static string TAG = "CREATE_TRIP_DIALOG_FRAGMENT";

        public event EventHandler<CreateTripDialogEventArgs> CreateTripDialogClosed;      // Event handler when user presses create
        private Button _createTripButton;                                // Create pin button
        private Button _cancelButton;                                   // Cancel create trip button
        private EditText _name;                                         // Name user will put
        private EditText _type;                                         // Type of trip
        private EditText _description;                                  // Description user will put
        private EditText _cost;                                         // Amount user spent
        private EditText _pin;                                        // Search for a particular pin/activity
       // private Array _pinsList;                                        // List of pins for the trip
        private bool _createTrip;                                        // Check if they pressed create or not

        public static CreateTripDialogFragment NewInstance()
        {
            CreateTripDialogFragment fragment = new CreateTripDialogFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.create_trip_dialog_fragment, container, false);
            _createTripButton = (Button)view.FindViewById(Resource.Id.create_trip_dialog_create_button);
            _cancelButton = (Button)view.FindViewById(Resource.Id.create_trip_dialog_cancel_button);
            _name = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_name);
            _type = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_type);
            _description = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_text_description);
            _cost = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_cost);
            _pin = (EditText)view.FindViewById(Resource.Id.create_trip_dialog_search);

            //_pin.KeyPress += (object sender, View.KeyEventArgs e) => {
            //    e.Handled = false;
            //    if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            //   {
            //        Toast.MakeText(Context, _pin.Text, ToastLength.Short).Show();
            //        e.Handled = true;
            //    }
            //};

            this.Dialog.SetCancelable(true);
            this.Dialog.SetCanceledOnTouchOutside(true);
            
            SetListeners();
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);

            // Store and output data to the parent fragment
            if (CreateTripDialogClosed != null && _createTrip)
            {
                CreateTripDialogClosed(this, new CreateTripDialogEventArgs
                {
                    Name = _name.Text,
                    Type = _type.Text,
                    Description = _description.Text,
                    Cost = Convert.ToDouble(_cost.Text),
                    //Pins = _pinsList,
                    CreateDate = DateTime.Today,
                });

                MainActivity.UpdateAccount(MainActivity.GetAccount().Id, 1, 1);
            }
        }

        private void SetListeners()
        {
            _createTripButton.Click += (sender, e) =>
            {
                Toast.MakeText(this.Activity, "Created Trip", ToastLength.Short).Show();
                _createTrip = true;
                Dialog.Dismiss();
            };

            _cancelButton.Click += (sender, e) =>
            {
                _createTrip = false;
                Dialog.Dismiss();
            };
        }
        }
    public class CreateTripDialogEventArgs
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        //public Array Pins { get; set; }
        public DateTime CreateDate { get; set; }
    }
}