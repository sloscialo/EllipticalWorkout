using System;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace EllipticalWorkout
{
	[Activity (Label = "Elliptical Workout", 
	           MainLauncher = true, 
	           Icon="@drawable/workout_icon",
	           ConfigurationChanges=ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : ActivityBase
	{
		public MainActivity() : base(Resource.Id.fragmentView)
		{
			// Allows base class to accept a resource Id without having to hard-code a name.
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);

			RegisterServices();

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Load initial fragment.
			var wl = new WorkoutListFragment();
			PushFragment(wl);
		}
		
		private void RegisterServices()
		{
			// Register services.
			//ServiceFactory.Instance.Register<IWorkoutData>(workoutData);
			ServiceFactory.Instance.Register<IWorkoutDataSource>(new RemoteJsonDataSource());
			ServiceFactory.Instance.Register<Context>(ApplicationContext);
			ServiceFactory.Instance.Register<IEventAggregator>(new EventAggregatorService());
			ServiceFactory.Instance.Register<IWorkoutTimer>(new WorkoutTimer());
		}
	}
}


