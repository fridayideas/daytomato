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
			vh.SuggestionName.Text = _suggestions[position].Name;
			vh.SuggestionType.Text = _suggestions[position].Type;
		}
	}

	public class TripSuggestionViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView SuggestionName { get; private set; }
		public TextView SuggestionType { get; private set; }

		public TripSuggestionViewHolder(View itemView) : base(itemView)
		{
			SuggestionName = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_name);
			SuggestionType = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_type);
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
