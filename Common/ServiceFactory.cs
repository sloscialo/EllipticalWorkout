
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
	public class ServiceFactory
	{
		private static ServiceFactory _instance = null;
		private static readonly Dictionary<Type, object> _catalog;

		static ServiceFactory ()
		{
			_catalog = new Dictionary<Type, object>();
		}

		private ServiceFactory ()
		{
			
		}

		public static ServiceFactory Instance
		{
			get 
			{
				if (_instance == null)
					_instance = new ServiceFactory();

				return _instance;
			}
		}

		public void Register<T>() 
		{
			var t = typeof(T);

			if (!_catalog.ContainsKey(t))
				_catalog.Add(t, default(T));
		}

		public void Register<T>(T instance) 
		{
			var t = typeof(T);

			if (!_catalog.ContainsKey(t))
				_catalog.Add(t, instance);
		}

		public T Resolve<T>() 
		{
			var t = typeof(T);
			if (!_catalog.ContainsKey(t)) return default(T);

			return (T)_catalog[t];
		}
	}
}

