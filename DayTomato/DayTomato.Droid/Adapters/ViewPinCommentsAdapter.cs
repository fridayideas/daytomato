using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using Android.App;

namespace DayTomato.Droid
{
	public class ViewPinCommentsAdapter : BaseAdapter<Comment>
	{
		private List<Comment> _comments;

		private Activity _context;

		public ViewPinCommentsAdapter(Activity context, List<Comment> comments) : base()
		{
			_context = context;
			_comments = comments;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { return _comments.Count; }
		}

		public override Comment this[int position]
		{
			get
			{
				return _comments[position];
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = _context.LayoutInflater.Inflate(Resource.Layout.pin_comment_view_holder, null);
			
			view.FindViewById<TextView>(Resource.Id.pin_comment_text).Text = _comments[position].Text;
			view.FindViewById<TextView>(Resource.Id.pin_comment_text).Text += " - ";
			view.FindViewById<TextView>(Resource.Id.pin_comment_text).Text += _comments[position].LinkedAccount;
			view.FindViewById<TextView>(Resource.Id.pin_comment_text).Text += " ";
			view.FindViewById<TextView>(Resource.Id.pin_comment_text).Text += _comments[position].CreateDate.ToShortDateString();

			return view;
		}
	}
}