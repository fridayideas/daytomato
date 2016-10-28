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
using System.Threading.Tasks;
using System.Linq;
using Java.Lang;

namespace DayTomato.Droid
{
    public class AddPinsDialogFragment : DialogFragment
    {
        private readonly static string TAG = "ADD_PINS_DIALOG_FRAGMENT";

        public event EventHandler<AddPinsDialogEventArgs> AddPinsDialogClosed;      // Event handler when user presses create
        private Button _createTripButton;                               // Create trip button
        private Button _cancelButton;                                   // Cancel create trip button
        private Button _addPinButton;                                   // Add pin to trip button
        private List<string> _addedPins = new List<string> ();                                  // Array of added pins
        private List<Pin> _allPins;                                     // All pins currently on server
        private AutoCompleteTextView autocompleteTextView;              // Search for pins
        private TextView _listPins;                                     // List of added pins
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
            _cancelButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_cancel_button);
            _addPinButton = (Button)view.FindViewById(Resource.Id.add_pins_dialog_add_button);
            
            _listPins = view.FindViewById<TextView>(Resource.Id.add_pins_dialog_listpins);

            // Anytime the user clicks on the add button, add that pin to the trip and display the list of pins on the screen
            _addPinButton.Click += (sender, e) =>
            {
                if (autocompleteTextView.Text != null)
                {
                    Pin _pin = _allPins.Find(p => p.Name == autocompleteTextView.Text);
                    if (_pin != null)
                    {
                        _addedPins.Add(_pin.Id);
                        Toast.MakeText(this.Activity, "Added Pin", ToastLength.Short).Show();
                        _listPins.Text += autocompleteTextView.Text + "\n";
                        _pin = null;
                    }
                    else { Toast.MakeText(this.Activity, "Could not find pin", ToastLength.Short).Show(); }
                }
                else { Toast.MakeText(this.Activity, "Type the name of a pin to add", ToastLength.Short).Show(); }
            };

            ArrayAdapter autoCompleteAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());

            autocompleteTextView = view.FindViewById<AutoCompleteTextView>(Resource.Id.add_pins_dialog_autocomplete);
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
                    Pins = _addedPins
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
        public List<string> Pins { get; set; }
    }
}