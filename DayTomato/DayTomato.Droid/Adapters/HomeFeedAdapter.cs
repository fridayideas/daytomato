using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace DayTomato.Droid
{
	public class HomeFeedAdapter : RecyclerView.Adapter
	{
		private List<string> _feed;

		public HomeFeedAdapter(List<string> feed)
		{
			_feed = feed;
		}

		public override int ItemCount
		{
			get { return _feed.Count; }
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// Inflate the viewholder
			View itemView = LayoutInflater.From(parent.Context).
						Inflate(Resource.Layout.home_feed_view_holder, parent, false);

			// Create a ViewHolder to hold view references inside the CardView
			HomeFeedViewHolder vh = new HomeFeedViewHolder(itemView);
			return vh;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			HomeFeedViewHolder vh = holder as HomeFeedViewHolder;
			vh.FeedInformation.Text = _feed[position];
		}
	}

	public class HomeFeedViewHolder : RecyclerView.ViewHolder
	{
		private Action<int> _listener;

		public TextView FeedInformation { get; private set; }

		public HomeFeedViewHolder(View itemView) : base(itemView)
		{
			FeedInformation = itemView.FindViewById<TextView>(Resource.Id.home_feed_action);
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
