
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using DayTomato.Models;
using Android.App;

namespace DayTomato.Droid
{
	public class CommentsAdapter : BaseAdapter<Comment>
	{
		private readonly List<Comment> _comments;
		private readonly Activity _context;

		public CommentsAdapter(Activity context, List<Comment> comments) : base()
		{
			_context = context;
			_comments = comments ?? new List<Comment>();
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count => _comments.Count;

	    public override Comment this[int position] => _comments[position];

	    public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = _context.LayoutInflater.Inflate(Resource.Layout.comment_view_holder, null);

			TextView comment = view.FindViewById<TextView>(Resource.Id.comment_text);
			
			comment.Text = _comments[position].Text;
			comment.Text += " - ";
			comment.Text += _comments[position].Username;
			comment.Text += " ";
			comment.Text += _comments[position].CreateDate.ToShortDateString();

			return view;
		}
	}
}