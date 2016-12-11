
using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.App;

namespace DayTomato.Droid
{
	public class FilterDialogFragment : DialogFragment
	{
		private readonly static string TAG = "FILTER_DIALOG_FRAGMENT";

		public event EventHandler<FilterDialogEventArgs> FilterDialogClosed;      // Event handler when user presses create
		private Button _finishButton;                                   // finish editting pin button
		private Button _cancelButton;                                   // Cancel editting pin button

		private List<CheckBox> _boxes;

		private CheckBox _all;
		private CheckBox _general;
		private CheckBox _food;
		private CheckBox _poi;
		private CheckBox _shopping;
		private CheckBox _outdoors;

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
			_all = (CheckBox)view.FindViewById(Resource.Id.filter_dialog_all);
			_boxes = new List<CheckBox>();

			_boxes.Add(_general);
			_boxes.Add(_food);
			_boxes.Add(_poi);
			_boxes.Add(_shopping);
			_boxes.Add(_outdoors);

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
						_outdoors.Checked
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
			_all.Checked = SetAllChecked();
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

			_all.Click += (sender, e) =>
			{
				_all.Checked = !_all.Checked;
				if (_all.Checked)
				{
					ToggleAllChecked(false);
				}
				else
				{
					ToggleAllChecked(true);
				}
			};

			foreach (var b in _boxes)
			{
				b.Click += (sender, e) =>
				{
					if (_all.Checked)
					{
						ToggleAllChecked(false);
						b.Checked = true;
					}
				};
			}

		}
		private void ToggleAllChecked(bool check)
		{
			_all.Checked = check;
			_general.Checked = check;
			_food.Checked = check;
			_poi.Checked = check;
			_shopping.Checked = check;
			_outdoors.Checked = check;
		}

		private bool SetAllChecked()
		{
			foreach(var b in _boxes)
			{
				if (!b.Checked)
					return false;
			}
			return true;
		}
	}

	public class FilterDialogEventArgs
	{
		public bool[] FilterOptions { get; set; }
		public bool Filter { get; set; }
	}
}
