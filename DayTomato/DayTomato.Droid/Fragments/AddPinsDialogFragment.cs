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
using DayTomato.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayTomato.Droid
{
    public class AddPinsDialogFragment : DialogFragment
    {
        private readonly static string TAG = "ADD_PINS_DIALOG_FRAGMENT";

        public event EventHandler<AddPinsDialogEventArgs> AddPinsDialogClosed;      // Event handler when user presses create
        private Button _createTripButton;                               // Create trip button
        private Button _cancelButton;                                   // Cancel create trip button
        private EditText _pin;                                          // Search for a particular pin/activity
        private List<Pin> _added_pins;                                      // Array of added pins
        private List<Pin> _allPins;                                     // All pins currently on server
        private bool _createTrip;                                        // Check if they pressed create or not

        public static AddPinsDialogFragment NewInstance()
        {
            AddPinsDialogFragment fragment = new AddPinsDialogFragment();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.add_pins_dialog_fragment, container, false);
            _createTripButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_create_button);
            _cancelButton = (Button)view.FindViewById(Resource.Id.create_trip_dialog_cancel_button);

            ArrayAdapter autoCompleteAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());

            var autocompleteTextView = view.FindViewById<AutoCompleteTextView>(Resource.Id.add_pins_dialog_autocomplete);
            // Minimum number of characters to begin autocompletet for
            autocompleteTextView.Threshold = 1;
            autocompleteTextView.Adapter = autoCompleteAdapter;

            // Load all pins from the server
            Task.Run(async () =>
            {
                _allPins = await MainActivity.dayTomatoClient.GetPins();
                autoCompleteAdapter.AddAll(_allPins.Select(p => p.Name).ToList());
                // Updates autocomplete data
                autoCompleteAdapter.NotifyDataSetChanged();
            });

            /*_pin.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Space)
               {
                    Toast.MakeText(this.Activity, _pin.Text, ToastLength.Short).Show();
                    e.Handled = true;
                }
            };*/


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
            if (AddPinsDialogClosed != null && _createTrip)
            { 
                AddPinsDialogClosed(this, new AddPinsDialogEventArgs
                {
                    Pins = _added_pins
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
    public class AddPinsDialogEventArgs
    {
        public List<Pin> Pins { get; set; }
    }
}