using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;

namespace DayTomato.Droid
{
	public class TripSuggestionAdapter : RecyclerView.Adapter
	{
		private List<Trip> _suggestions;

		public TripSuggestionAdapter(List<Trip> suggestions)
		{
			_suggestions = suggestions;
		}

		public override int ItemCount
		{
			get { return _suggestions.Count; }
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			View itemView = LayoutInflater.From(parent.Context).
						Inflate(Resource.Layout.trip_suggestion_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			TripSuggestionViewHolder vh = new TripSuggestionViewHolder(itemView);
			return vh;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			TripSuggestionViewHolder vh = holder as TripSuggestionViewHolder;
			vh.Name.Text = _suggestions[position].Name;
			vh.Type.Text = _suggestions[position].Type;
			foreach (var p in _suggestions[position].Pins)
			{
				vh.Pins.Text += p.Name + "\n";
			}
			vh.CreateDate.Text = "created " + _suggestions[position].CreateDate.ToLongDateString();
			vh.Author.Text = _suggestions[position].LinkedAccount;
		}
	}

	public class TripSuggestionViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView Name { get; private set; }
		public TextView Type { get; private set; }
		public TextView Pins { get; private set; }
		public TextView CreateDate { get; private set; }
		public TextView Author { get; private set; }

		public TripSuggestionViewHolder(View itemView) : base(itemView)
		{
			Name = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_name);
			Type = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_type);
			Pins = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_pins);
			CreateDate = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_createdate);
			Author = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_author);
		}

		public void SetClickListener(Action<int> listener)
		{
			_listener = listener;
			ItemView.Click += HandleClick;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (ItemView != null)
			{
				ItemView.Click -= HandleClick;
			}
			_listener = null;
		}

		void HandleClick(object sender, EventArgs e)
		{
			_listener?.Invoke(base.AdapterPosition);
		}
	}
}
