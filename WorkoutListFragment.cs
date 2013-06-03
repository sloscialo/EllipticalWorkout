using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace EllipticalWorkout
{
	public class WorkoutListFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try 
			{
				// Create MainViewModel
				var view = inflater.Inflate(Resource.Layout.WorkoutListFragmentView, container, false);
				var vm = new MainViewAdapter(inflater);
				var workoutListView = view.FindViewById<ListView>(Resource.Id.workoutsView);

				workoutListView.Adapter = vm;
				workoutListView.ItemClick += HandleItemClick;

				return view;
			}
			catch
			{
				return base.OnCreateView(inflater, container, savedInstanceState);
			}
		}

		private async void HandleItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			var view = sender as ListView;
			var vm = view.Adapter as MainViewAdapter;
			var w = vm[e.Position];

			// Get stage data for workout.
			var wds = ServiceFactory.Instance.Resolve<IWorkoutDataSource>();

			// This populates the stage data for the workout if necessary.
			if (w.Stages.Count == 0)
				w = await wds.GetWorkoutById(w.ExerciseType, w.Id);

			// Set WorkoutTimer
			var wt = ServiceFactory.Instance.Resolve<IWorkoutTimer>();
			wt.SetWorkout(w);

			var activity = Activity as MainActivity;
			if (activity == null)
				throw new ApplicationException("Problem obtaining Main Activity.");

			activity.PushFragment(new WorkoutFragment(w));
		}
	}
}

