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
using System.Timers;

namespace EllipticalWorkout
{
	public class WorkoutMessage
	{
		public enum Status {
			Warning,
			Completed,
			StageChanged,
			TimerTicked
		}

		public WorkoutMessage(Status messageStatus)
		{
			this.MessageStatus = messageStatus;
		}

		public Status MessageStatus { get; private set; }
	}
	
}
