
using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using Newtonsoft.Json;
using System;
using Android.Graphics.Drawables;

namespace DayTomato.Droid
{
	public class ViewPinDialogFragment : DialogFragment
	{
		public event EventHandler<ViewPinDialogEventArgs> ViewPinDialogClosed;
		private List<Pin> _pins;
		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewPinAdapter _adapter;
		private TextView _title;
		private Button _return;
		private Button _add;
		private bool _create;
		bool quit = false;

		public static ViewPinDialogFragment NewInstance(Bundle bundle)
		{
			ViewPinDialogFragment fragment = new ViewPinDialogFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.view_pin_dialog_fragment, container, false);

			_pins = JsonConvert.DeserializeObject<List<Pin>>(Arguments.GetString("VIEW_PIN_DATA"));

			_return = view.FindViewById<Button>(Resource.Id.view_pin_dialog_return_button);
			_add = view.FindViewById<Button>(Resource.Id.view_pin_dialog_add_button);
			_title = view.FindViewById<TextView>(Resource.Id.view_pin_dialog_title);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.view_pin_recycler_view);
			_layoutManager = new LinearLayoutManager(Context);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewPinAdapter(_pins, Activity, this.Dialog);
			_recyclerView.SetAdapter(_adapter);

			this.Dialog.SetCancelable(true);
			this.Dialog.SetCanceledOnTouchOutside(true);

			SetInstances();
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
			if (ViewPinDialogClosed != null)
			{
				ViewPinDialogClosed(this, new ViewPinDialogEventArgs
				{
					Create = _create
				});
			}

		}
		public override void OnDestroyOptionsMenu()
		{
			base.OnDestroyOptionsMenu();
			Console.WriteLine("asd");
		}
		private void SetInstances()
		{
			_title.Text = Arguments.GetString("VIEW_PIN_TITLE");
			_create = false;
		}

		private void SetListeners()
		{
			_return.Click += (sender, e) => 
			{
				_create = false;
				Dialog.Dismiss();
			};
			_add.Click += (sender, e) => 
			{
				_create = true;
				Dialog.Dismiss();
			};
			_recyclerView.LayoutChange += (sender, e) =>
			{
				if (_adapter.ItemCount == 0)
				{
					_create = false;
					Console.WriteLine("sdf");
					Dialog.Dismiss();
				}
			};
		}
	}

	public class ViewPinDialogEventArgs
	{
		public bool Create { get; set; }
	}
}
