
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
		private bool _delete;
		private List<Pin> _update;

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
			_adapter = new ViewPinAdapter(_pins, Activity, this);
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
					Create = _create,
					Delete = _delete,
					Update = PinsToUpdate(),
					MarkerId = Arguments.GetLong("VIEW_PIN_MARKER")
				});
			}
		}

		private void SetInstances()
		{
			_title.Text = Arguments.GetString("VIEW_PIN_TITLE");
			_create = false;
			_delete = false;
			_update = new List<Pin>();
		}

		private void SetListeners()
		{
			_return.Click += (sender, e) => 
			{
				Dialog.Dismiss();
			};
			_add.Click += (sender, e) => 
			{
				_create = true;
				Dialog.Dismiss();
			};
			_recyclerView.ChildViewRemoved += (sender, e) =>
			{
				if (_adapter.ItemCount == 0)
				{
					if (Dialog != null)
					{
						_delete = true;
						Dialog.Dismiss();
					}
				}
			};
		}

		public List<Pin> PinsToUpdate()
		{
			foreach (var p in _adapter.GetItems())
			{
				_update.Add(p);
			}
			return _update;
		}

        public void EditPinDialog(string pinId, int position)
        {
            var fm = FragmentManager;
            var ft = fm.BeginTransaction();

            //Remove fragment else it will crash as it is already added to backstack
            var prev = fm.FindFragmentByTag("EditPinDialog");
            if (prev != null)
            {
                ft.Remove(prev);
            }

            ft.AddToBackStack(null);

			var bundle = new Bundle();
			bundle.PutString("EDIT_PIN_ID", pinId);
			bundle.PutInt("EDIT_PIN_POSITION", position);
            var editPinDialogFragment = EditPinDialogFragment.NewInstance(bundle);
            editPinDialogFragment.EditPinDialogClosed += OnEditPinDialogClosed;

            //Add fragment
            editPinDialogFragment.Show(fm, "EditPinDialog");
        }

        private async void OnEditPinDialogClosed(object sender, EditPinDialogEventArgs e)
        {
            var account = MainActivity.GetAccount();
            var pin = new Pin
            {
                Id = e.Id,
                Type = e.Type,
                Name = e.Name,
                Rating = e.Rating,
                Description = e.Description,
                Likes = e.Likes,
				Coordinate = new Coordinate(e.Latitude, e.Longitude),
                LinkedAccount = account.Id,
                Review = e.Review,
                Cost = e.Cost,
                CreateDate = e.CreateDate,
                ImageURL = e.ImageUrl,
                Comments = e.Comments,
				LikedBy = e.LikedBy,
				DislikedBy = e.DislikedBy
            };
            
            await MainActivity.dayTomatoClient.UpdatePin(pin);
            
            _pins[e.PinPosition] = pin;
            _adapter.NotifyDataSetChanged();
        }
    }

	public class ViewPinDialogEventArgs
	{
		public bool Create { get; set; }
		public bool Delete { get; set; }
		public List<Pin> Update { get; set; }
		public long MarkerId { get; set; }
	}
}
