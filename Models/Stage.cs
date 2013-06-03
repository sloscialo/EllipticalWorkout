
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

namespace EllipticalWorkout
{
	public class Stage
	{
		public int Id { get; set; }
		public TimeSpan Duration { get; set; }
		public Range<int> CrossRamp { get; set; }
		public Range<int> Resistance { get; set; }
		public Range<int> Strides { get; set; }
		public string Message { get; set; }
	}
}
