
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

namespace EllipticalWorkout
{
	public class WorkoutDataAdapter : IWorkoutData
	{
		private const string WorkoutFileName = "workouts.xml";
		private static List<Workout> _workouts;
		private readonly AssetManager _assetManager;

		public WorkoutDataAdapter (AssetManager assets)
		{
			_assetManager = assets;

			var localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			var localFile = Path.Combine(localFolder, WorkoutFileName);
					
			// Check if user file exists.
			if (!File.Exists (localFile))
			{
				// Check if folder exists.
				if (!Directory.Exists(localFolder)) {
					Directory.CreateDirectory(localFolder);
				}

				using (var defaultData = _assetManager.Open("Workouts.xml"))
				{
					// Save default workouts to folder.
					using (var fs = new FileStream(localFile, FileMode.CreateNew, FileAccess.Write))
					{
						defaultData.CopyTo (fs);
						fs.Flush ();
						fs.Close ();
					}
					defaultData.Close ();
				}
			}

			// Now open local file.
			var dataStream = new FileStream(localFile, FileMode.Open, FileAccess.Read);

			LoadWorkouts (dataStream);
		}
		
		public Workout GetWorkoutById(string id)
		{
			var q = from w in _workouts
					where w.Id.Equals (id, StringComparison.CurrentCultureIgnoreCase)
					select w;

			return q.FirstOrDefault ();
		}

		public IList<Workout> Workouts
		{
			get
			{
				return _workouts;
			}
		}

		private Tuple<bool, bool> QueryExternalStorage()
		{
			var state = Android.OS.Environment.ExternalStorageState;
			var storageAvailable = false;
			var storageWriteable = false;

			if (Android.OS.Environment.MediaMounted.Equals (state))
			{
				storageAvailable = storageWriteable = Android.OS.Environment.MediaMounted.Equals (state);
			} else
			{
				if (Android.OS.Environment.MediaMountedReadOnly.Equals (state))
				{
					storageAvailable = true;
				}
			}

			return new Tuple<bool, bool> (storageAvailable, storageWriteable);
		}

		private void LoadWorkouts(Stream workoutData)
		{
			var doc = XDocument.Load (workoutData);
			var q = from w in doc.Descendants ("workout")
				select new Workout () 
			{
				Id = w.TryGetValue("id", "0"),
				Name = w.TryGetValue("name", "Unknown"),
				Level = (WorkoutLevel) Enum.Parse(typeof(WorkoutLevel), w.TryGetValue("level", "Low")),
				Stages = (from seg in w.Elements("stage")
				          select new Stage() 
				          	{
								Id = int.Parse(seg.TryGetValue("id", "0")),
								Duration = TimeSpan.Parse(seg.TryGetValue("duration", "0:00")),
								CrossRamp = GetRangeElement("crossRamp", seg),
								Resistance = GetRangeElement("resistance", seg),
								Strides= GetRangeElement("strides", seg), 
								Message = seg.TryGetValue("message", string.Empty)
							}).ToList()			        
			};
			_workouts = q.ToList ();
		}

		private Range<int> GetRangeElement(string attributeBase, XElement fragment)
		{
			if (!fragment.HasAttributes)
				return null;

			// Check for crossramp
			var attrValue = fragment.TryGetValue (attributeBase, null);
			return Range<int>.Parse (attrValue);
		}
	}
}
