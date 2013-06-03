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
	public interface IWorkoutTimer 
	{
		event EventHandler<WorkoutTimerTickEventArgs> Tick;

		void SetWorkout(Workout workout);
		void Stop();
		void Start();

		bool IsRunning { get; }
		TimeSpan RemainingTime { get; }
		TimeSpan Elapsed { get; }
		int CurrentStageIndex { get; }
		Stage CurrentStage { get; }
	}

	public class WorkoutTimerTickEventArgs : EventArgs
	{
		public WorkoutTimerTickEventArgs(WorkoutMessage message)
		{
			this.Message = message;
		}

		public WorkoutMessage Message { get; private set; }
	}

	public class WorkoutTimer : IWorkoutTimer
	{
		private const int WarningThreshold = 5;
		private readonly static object _synclock = new object();

		private IEventAggregator _eventAggregator;
		private Timer _timer;

		private Workout _workout;
		private int _currentStageIndex;
		private TimeSpan _remainingTime;
		private TimeSpan _elapsed;
		private bool _isWarning;
		private bool _isRunning;

		public event EventHandler<WorkoutTimerTickEventArgs> Tick;

		public WorkoutTimer()
		{
			_eventAggregator = ServiceFactory.Instance.Resolve<IEventAggregator>();

			_timer = new Timer();
			_timer.Interval = 1000;

			_isWarning = false;
			_isRunning = false;
			_currentStageIndex = 0;
		}
		
		private void HandleElapsed(object sender, ElapsedEventArgs e)
		{
			lock (_synclock)
			{
				this.RemainingTime -= TimeSpan.FromSeconds(1);
				this.Elapsed += TimeSpan.FromSeconds(1);
			}

			var tickType = WorkoutMessage.Status.TimerTicked;

			//_eventAggregator.Publish(new WorkoutMessage(WorkoutMessage.Status.TimerTicked));			

			// Send out the warning message;
			if (this.RemainingTime.TotalSeconds == WarningThreshold && !_isWarning)
			{
				//_eventAggregator.Publish(new WorkoutMessage(WorkoutMessage.Status.Warning));
				tickType = WorkoutMessage.Status.Warning;

				_isWarning = true;
			}

			if (this.RemainingTime.TotalSeconds <= 0)
			{
				if (this.CurrentStageIndex + 1 >= _workout.Stages.Count)
				{
					// We're finished.
					this.Stop();
					this.RemainingTime = TimeSpan.Zero;
					tickType = WorkoutMessage.Status.Completed;

					//_eventAggregator.Publish(new WorkoutMessage(WorkoutMessage.Status.Completed));
				}
				else
				{
					this.CurrentStageIndex++;

					_isWarning = false;

					this.RemainingTime = CurrentStage.Duration;

					//_eventAggregator.Publish(new WorkoutMessage(WorkoutMessage.Status.StageChanged));
					tickType = WorkoutMessage.Status.StageChanged;
				}
			}

			OnTick(tickType);
			_eventAggregator.Publish(new WorkoutMessage(tickType));
		}

		public void SetWorkout(Workout workout) 
		{
			this.Stop();
			_workout = workout;

			this.RemainingTime = workout.Stages.First().Duration;
			this.Elapsed = TimeSpan.Zero;

			_isWarning = false;
			_currentStageIndex = 0;
		}

		public void Stop() 
		{
			_timer.Stop();
			_isRunning = false;

			_timer.Elapsed -= HandleElapsed;
		}

		public void Start() 
		{
			_timer.Elapsed += HandleElapsed;
			_timer.Start();
			_isRunning = true;
		}

		public bool IsRunning
		{
			get
			{
				return _isRunning;
			}
		}

		public TimeSpan RemainingTime 
		{ 
			get
			{
				return _remainingTime;
			}
			private set 
			{
				_remainingTime = value;

				// Send notification?
			}
		}

		public TimeSpan Elapsed
		{
			get
			{
				return _elapsed;
			}
			private set 
			{
				_elapsed = value;

				// Send notifcation?
			}
		}

		public int CurrentStageIndex
		{
			get
			{
				return _currentStageIndex;
			}
			private set 
			{
				if (_workout == null)
					throw new ApplicationException("No workout set.");

				if (value < 0 || value >= _workout.Stages.Count)
					throw new ArgumentOutOfRangeException();

				_currentStageIndex = value;
			}
		}

		public Stage CurrentStage
		{
			get
			{
				if (_workout == null)
					return null;

				return _workout.Stages[this.CurrentStageIndex];
			}
		}

		private void OnTick(WorkoutMessage.Status status) 
		{
			var eh = this.Tick;
			if (eh != null)
			{
				eh(this, new WorkoutTimerTickEventArgs(new WorkoutMessage(status)));
			}
		}
	}
}

