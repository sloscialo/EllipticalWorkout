using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace EllipticalWorkout
{
	public class RemoteJsonDataSource : IWorkoutDataSource
	{
		private const string RemoteUrl = "http://98.221.140.21:1337/";	
		private IList<Workout> _cachedWorkouts = null;
		private DateTime _lastUpdated = DateTime.MinValue;

		public RemoteJsonDataSource()
		{
		}
	
		#region IWorkoutData implementation

		public async Task<Workout> GetWorkoutById(WorkoutType workoutType, int id)
		{
			var targetUrl = string.Format("{0}{1}/{2}", RemoteUrl, workoutType.ToString().ToLower(), id);
			var remoteUri = new Uri(targetUrl, UriKind.Absolute);

			if (_cachedWorkouts == null)
				return null;

			var workout = _cachedWorkouts.FirstOrDefault(w => w.Id == id);
			if (workout == null)
				return null;

			if (workout.Stages.Count > 0)
				return workout;

			// Populates the stages collection with new data.
			try
			{
				var stages = new List<Stage>();
				var httpRequest = (HttpWebRequest) WebRequest.Create(remoteUri);
				httpRequest.ContentType = "application/json";
				httpRequest.Method = "GET";

				using (var response = await httpRequest.GetResponseAsync() as HttpWebResponse) 
				{
					if (response.StatusCode != HttpStatusCode.OK)
					{
						Debug.WriteLine("Error fetching data from server.");
						return null;
					}

					var s = response.GetResponseStream();
					if (s != null)
					{
						var j = JsonObject.Load(s);

						// Create workouts from json data.
						var results = from result in j[0] as JsonArray
							let jresult = result as JsonObject
								select new Stage() 
							{
								Id = jresult["stageId"],
								Duration = TimeSpan.Parse(jresult["duration"]),
								CrossRamp = Range<int>.Parse(jresult["crossRamp"]),
								Resistance = Range<int>.Parse(jresult["resistance"]),
								Strides = Range<int>.Parse(jresult["strides"]),
								Message = jresult["message"]
							};

						stages = results.ToList();

						workout.Stages = stages;
					}
				}

				return workout;
			} 
			catch (Exception ex)
			{
				Debug.WriteLine (ex.Message);
				return null;
			}
		}

		public async Task<IList<Workout>> GetWorkouts(WorkoutType workoutType)
		{
			//if (_cachedWorkouts != null && (DateTime.Now - _lastUpdated).TotalHours <= 4)
			//	return _cachedWorkouts;

			var targetUrl = string.Concat (RemoteUrl, workoutType.ToString().ToLower());
			var remoteUri = new Uri(targetUrl, UriKind.Absolute);
			var workouts = new List<Workout>();

			try
			{
				var httpRequest = (HttpWebRequest) WebRequest.Create(remoteUri);
				httpRequest.ContentType = "application/json";
				httpRequest.Method = "GET";

				using (var response = await httpRequest.GetResponseAsync() as HttpWebResponse) 
				{
					if (response.StatusCode != HttpStatusCode.OK)
					{
						Debug.WriteLine("Error fetching data from server.");
						return null;
					}

					var s = response.GetResponseStream();
					if (s != null)
					{
						var j = JsonObject.Load(s);

						// Create workouts from json data.
						var results = from result in j[0] as JsonArray
							let jresult = result as JsonObject
								select new Workout() 
							{
								Id = jresult["id"],
								DisplayId = jresult["displayId"],
								Name = jresult["name"],
								ExerciseType = workoutType,
								Level = (WorkoutLevel) Enum.Parse(typeof(WorkoutLevel), jresult["level"])
							};

						_cachedWorkouts = results.ToList();
						_lastUpdated = DateTime.Now;

						return _cachedWorkouts;
					}
				}

				return workouts;
			} 
			catch (Exception ex)
			{
				Debug.WriteLine (ex.Message);
				return null;
			}
		}
		#endregion

	}

	public static class HttpWebRequestExtensions
	{
		public static Task<WebResponse> GetResponseAsync(this WebRequest request)
		{
            return Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
		}
	}
}

