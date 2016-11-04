
using System;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace DayTomato.Droid
{
	public class FilterDialogFragment : DialogFragment
	{
		private readonly static string TAG = "FILTER_DIALOG_FRAGMENT";

		public event EventHandler<FilterDialogEventArgs> FilterDialogClosed;      // Event handler when user presses create
		private Button _finishButton;                                   // finish editting pin button
		private Button _cancelButton;                                   // Cancel editting pin button

		private CheckBox _food;
		private CheckBox _poi;
		private CheckBox _shopping;
		private CheckBox _outdoors;
		private CheckBox _general;
		private CheckBox _cultural;
		private CheckBox _kids;
		private CheckBox _walking;
		private CheckBox _biking;
		private CheckBox _driving;
		private CheckBox _budget;

		private bool _filter;
		private bool[] _filterOptions;

		public static FilterDialogFragment NewInstance(Bundle bundle)
		{
			FilterDialogFragment fragment = new FilterDialogFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.filter_dialog_fragment, container, false);
			_finishButton = (Button)view.FindViewById(Resource.Id.filter_dialog_filter);
			_cancelButton = (Button)view.FindViewById(Resource.Id.filter_dialog_cancel);
			_food = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_food);
			_poi = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_poi);
			_shopping = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_shopping);
			_outdoors = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_outdoors);
			_general = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_general);
			_cultural = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_cultural);
		 	_kids = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_kids);
			_walking = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_walking);
			_biking = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_biking);
			_driving = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_driving);
			_budget = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_budget);

			_filter = false;

			Dialog.SetCancelable(true);
			Dialog.SetCanceledOnTouchOutside(true);

			SetInstances();
			SetListeners();
			return view;
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		public override void OnDismiss(IDialogInterface dialog)
		{
			base.OnDismiss(dialog);

			// Store and output data to the parent fragment
			if (FilterDialogClosed != null && _filter)
			{

				FilterDialogClosed(this, new FilterDialogEventArgs
				{
					FilterOptions = new bool[] {
						_general.Checked,
						_food.Checked,
						_poi.Checked,
						_shopping.Checked,
						_outdoors.Checked,
						_cultural.Checked,
						_kids.Checked,
						_walking.Checked,
						_biking.Checked,
						_driving.Checked,
						_budget.Checked
					},
					Filter = _filter
				});
			}
		}

		private void SetInstances()
		{
			_filterOptions = Arguments.GetBooleanArray("FILTER_OPTIONS");
			_general.Checked = _filterOptions[0];
			_food.Checked = _filterOptions[1];
			_poi.Checked = _filterOptions[2];
			_shopping.Checked =_filterOptions[3];
			_outdoors.Checked = _filterOptions[4];
			_cultural.Checked = _filterOptions[5];
			_kids.Checked = _filterOptions[6];
			_walking.Checked = _filterOptions[7];
			_biking.Checked = _filterOptions[8]; 
			_driving.Checked = _filterOptions[9];
			_budget.Checked = _filterOptions[10];
		}

		private void SetListeners()
		{
			_finishButton.Click += (sender, e) =>
			{
				_filter = true;
				Dialog.Dismiss();
			};

			_cancelButton.Click += (sender, e) =>
			{
				_filter = false;
				Dialog.Dismiss();
			};
		}
	}

	public class FilterDialogEventArgs
	{
		public bool[] FilterOptions { get; set; }
		public bool Filter { get; set; }
	}
}
