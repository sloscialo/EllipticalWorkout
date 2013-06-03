
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

	public static class XElementExtensions
	{
		public static string TryGetValue(this XElement element, string attributeName, string nullValue) 
		{
			var attribute = element.Attribute(attributeName);
			if (attribute == null) return nullValue;

			return attribute.Value;
		}
	}
}
