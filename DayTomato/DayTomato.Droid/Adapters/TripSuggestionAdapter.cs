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
		//Create an Event so that our our clients can act when a user clicks
		//on each individual item.
		public event EventHandler<int> HandleClick;
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
			TripSuggestionViewHolder vh = new TripSuggestionViewHolder(itemView, OnClick);
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

		//This will fire any event handlers that are registered with our ItemClick
		//event.
		private void OnClick(int position)
		{
			if (HandleClick != null)
			{
				HandleClick(this, position);
			}
		}
	}

	public class TripSuggestionViewHolder : RecyclerView.ViewHolder
	{
		public TextView Name { get; private set; }
		public TextView Type { get; private set; }
		public TextView Pins { get; private set; }
		public TextView CreateDate { get; private set; }
		public TextView Author { get; private set; }

		public TripSuggestionViewHolder(View itemView, Action<int> listener) : base(itemView)
		{
			Name = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_name);
			Type = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_type);
			Pins = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_pins);
			CreateDate = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_createdate);
			Author = itemView.FindViewById<TextView>(Resource.Id.trip_suggestion_author);

			itemView.Click += (sender, e) => listener(AdapterPosition);
		}
	}
}
