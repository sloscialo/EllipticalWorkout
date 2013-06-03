
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
using Android.Content.Res;
using System.Threading.Tasks;

namespace EllipticalWorkout
{
	public interface IWorkoutDataSource
	{
		Task<Workout> GetWorkoutById(WorkoutType workoutType, int id);
		Task<IList<Workout>>  GetWorkouts(WorkoutType type);
	}
}
