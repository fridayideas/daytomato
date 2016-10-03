
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using DayTomato.Models;
using Newtonsoft.Json;

namespace DayTomato.Droid
{
	[Activity(Label = "ViewPin")]
	public class ViewPin : Activity
	{
		private List<Pin> _pins;
		private RecyclerView _recyclerView;
		private RecyclerView.LayoutManager _layoutManager;
		private ViewPinAdapter _adapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.view_pin);

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);

			_pins = JsonConvert.DeserializeObject<List<Pin>>(Intent.GetStringExtra("VIEW_PIN_DATA"));

			// Best rated pin gets title
			string title = Intent.GetStringExtra("VIEW_PIN_TITLE");
			toolbar.Title = title;

			_recyclerView = FindViewById<RecyclerView>(Resource.Id.view_pin_recycler_view);
			_layoutManager = new LinearLayoutManager(this);
			_recyclerView.SetLayoutManager(_layoutManager);
			_adapter = new ViewPinAdapter(_pins);
			_recyclerView.SetAdapter(_adapter);
		}
	}
}
