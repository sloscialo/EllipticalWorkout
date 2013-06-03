
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Runtime.Serialization;

namespace EllipticalWorkout
{
	public class Workout
	{
		public int Id { get; set; }
		public string DisplayId { get; set; }
		public string Name { get; set; }
		public List<Stage> Stages { get; set; }
		public WorkoutLevel Level { get; set; }
		public WorkoutType ExerciseType { get; set; }

		public Workout ()
		{
			this.Stages = new List<Stage>();
		}

		public TimeSpan Duration 
		{
			get 
			{
				if (this.Stages.Count == 0) return TimeSpan.Zero;

				return this.Stages.Aggregate(TimeSpan.Zero, (total, stage) => total + stage.Duration);
			}
		}
	}
}

