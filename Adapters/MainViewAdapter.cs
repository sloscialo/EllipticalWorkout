
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace EllipticalWorkout
{
	public class MainViewAdapter : BaseAdapter<Workout>
	{
		private LayoutInflater _inflater;
		private IWorkoutDataSource _workoutAdapter;
		private IList<Workout> _cachedWorkouts;

		public MainViewAdapter(LayoutInflater inflater) : base()
		{
			//_context = context;
			_workoutAdapter = ServiceFactory.Instance.Resolve<IWorkoutDataSource>();
			_inflater = inflater;

			LoadWorkouts();
		}

		private async void LoadWorkouts() 
		{
			if (_workoutAdapter == null)
				return;

			_cachedWorkouts = await _workoutAdapter.GetWorkouts(WorkoutType.Elliptical);

			NotifyDataSetChanged();
		}

		#region Overrides

		public override long GetItemId (int position)
		{
			return position;
		}

		public override Workout this[int position] 
		{
			get 
			{
				if (_cachedWorkouts == null)
					return null;

				// TODO: Check for out of bounds.
				return _cachedWorkouts[position];
			}
		}

		public override int Count 
		{
			get 
			{
				if (_cachedWorkouts == null)
					return 0;
			
				return _cachedWorkouts.Count;
			}
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			if (view == null)
				view = _inflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);

			var item = this[position];

			view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = string.Format("{0} - {1}", item.DisplayId, item.Name);

			return view;
		}
		#endregion
	}		
}

