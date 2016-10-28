
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
using System.Threading.Tasks;

namespace DayTomato.Droid
{
	public class ViewTripDialogFragment : DialogFragment
	{
		public event EventHandler<ViewTripDialogEventArgs> ViewTripDialogClosed;
		private Trip _trip;
		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewPinAdapter _adapter;
		private TextView _title;
		private Button _return;
		private Button _directions;

		public static ViewTripDialogFragment NewInstance(Bundle bundle)
		{
			ViewTripDialogFragment fragment = new ViewTripDialogFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.view_trip_dialog_fragment, container, false);

			_trip = JsonConvert.DeserializeObject<Trip>(Arguments.GetString("VIEW_TRIP_DATA"));

			_directions = view.FindViewById<Button>(Resource.Id.view_trip_dialog_map_button);
			_return = view.FindViewById<Button>(Resource.Id.view_trip_dialog_return_button);
			_title = view.FindViewById<TextView>(Resource.Id.view_trip_dialog_title);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.view_trip_recycler_view);
			_layoutManager = new LinearLayoutManager(Context);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewPinAdapter(_trip.Pins, Activity);
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
			if (ViewTripDialogClosed != null)
			{
				ViewTripDialogClosed(this, new ViewTripDialogEventArgs
				{
				});
			}
		}

		private void SetInstances()
		{
			_title.Text = Arguments.GetString("VIEW_TRIP_TITLE");
		}

		private void SetListeners()
		{
			_return.Click += (sender, e) =>
			{
				Dialog.Dismiss();
			};
			_directions.Click += (sender, e) => 
			{
				string coords = "";
				foreach (var p in _trip.Pins)
				{
					coords += "/" + p.Coordinate.latitude + "," + p.Coordinate.longitude;
				}
				var geoUri = Android.Net.Uri.Parse("https://www.google.com/maps/dir/" + coords);
				var mapIntent = new Intent(Intent.ActionView, geoUri);
				StartActivity(mapIntent);
			};
		}
	}

	public class ViewTripDialogEventArgs
	{
	}
}
