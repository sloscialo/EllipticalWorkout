
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
	public class Range<T>
	{
		private const string DefaultFormat = "{0} <small>to</small> {1}";

		public T Minimum { get; set; }

		public T Maximum { get; set; }

		public Range (T minimum, T maximum)
		{
			var c = Comparer<T>.Default.Compare (minimum, maximum);

			if (c > 0) 
				throw new ArgumentOutOfRangeException ("minimum", "value must be less or equal to maximum.");

			this.Minimum = minimum;
			this.Maximum = maximum;
		}

		public Range (T value) : this(value, value)
		{

		}
		
		public override string ToString()
		{
			if (EqualityComparer<T>.Default.Equals (this.Minimum, this.Maximum))
				return this.Minimum.ToString ();

			if (EqualityComparer<T>.Default.Equals (this.Minimum, default(T)))
			{
				return string.Format ("< {0}", this.Maximum);
			}

			if (EqualityComparer<T>.Default.Equals (this.Maximum, default(T)))
			{
				return string.Format ("> {0}", this.Minimum);
			}

			return string.Format (DefaultFormat, this.Minimum, this.Maximum);
		}

		public static Range<T> Parse(string value)
		{
			// Return null here?
			if (string.IsNullOrEmpty(value)) 
				return new Range<T>(default(T), default(T));

			try
			{
				if (!value.Contains ('-'))
				{
					T parsed = (T)Convert.ChangeType (value.Trim(), typeof(T));

					return new Range<T>(parsed);
				}

				var values = value.Split (new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
				if (values.Count () != 2)
					return null;

				var min = (T)Convert.ChangeType (values [0].Trim (), typeof(T));
				var max = (T)Convert.ChangeType (values [1].Trim (), typeof(T));

				return new Range<T> (min, max);
			} catch
			{
				return null;
			}
		}
	}
	
}
