
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
using Android.Animation;
using Android.Graphics;
using Android.Views.Animations;
using Android.Content.PM;
using Android;
using Android.Content.Res;
using Android.Text;

namespace EllipticalWorkout
{
//	[Activity (Label = "Elliptical Workout", 
//	           Icon="@drawable/workout_icon",
//	           ConfigurationChanges=ConfigChanges.ScreenSize | ConfigChanges.Orientation)]	
	public class WorkoutFragment : Fragment, IEventSink<WorkoutMessage>
	{
		private readonly Color DefaultColor = Color.Argb (0x9f, 0x00, 0x00, 0x20);
		private readonly Color WarningColor = Color.Argb (0x6f, 0x60, 0x00, 0x00);

		private readonly IWorkoutTimer _workoutTimer;
		private readonly IWorkoutDataSource _workoutData;
		private readonly IEventAggregator _eventAggregator;

		private Workout _currentWorkout;
		private bool _isFlashing = false;
		private ValueAnimator _warningAnimation;

		private TextView _timeRemainingInStageView;
		private TextView _elapsedTimeView;
		private TextView _currentStageView;
		private TextView _resistanceView;
		private TextView _crossrampView;
		private TextView _stridesView;
		private Button _startButton;
		private LinearLayout _remainingView;

		public WorkoutFragment(Workout workout)
		{
			_currentWorkout = workout;
			_workoutData = ServiceFactory.Instance.Resolve<IWorkoutDataSource> ();
			_workoutTimer = ServiceFactory.Instance.Resolve<IWorkoutTimer>();
			_eventAggregator = ServiceFactory.Instance.Resolve<IEventAggregator>();

			_eventAggregator.Subscribe(this);
		}
		
//		public override void Finish()
//		{
//			_workoutTimer.Stop();
//
//			if (_warningAnimation != null && _warningAnimation.IsRunning)
//				_warningAnimation.End();
//
//			_eventAggregator.Unsubscribe(this);
//
//			base.Finish();
//		}

		private void StartWarningAnimation(bool force = false)
		{
			if (!_isFlashing || force) {
				_isFlashing = true;
				this.Activity.RunOnUiThread (() => _warningAnimation.Start());
			}
		}

		private void StopWarningAnimation() 
		{
			if (_warningAnimation != null) 
			{
				this.Activity.RunOnUiThread(() => { 
					_warningAnimation.End();
					_remainingView.SetBackgroundColor(DefaultColor);
				});

				_isFlashing = false;
			}
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);

			//this.Activity.SetContentView(Resource.Layout.Workout);

			//UpdateUI();
			//CreateWarningAnimation();

			if (_isFlashing)
				StartWarningAnimation(true);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate(Resource.Layout.WorkoutFragmentView, container, false);

			UpdateUI(view);
			CreateWarningAnimation();

			return view;
		}
		

		private void CreateWarningAnimation() {
			// Create animation.
			int fromColor = DefaultColor.ToArgb();
			int toColor = WarningColor.ToArgb();

			_warningAnimation = ObjectAnimator.OfInt(_remainingView, "backgroundColor", fromColor, toColor);
			_warningAnimation.SetEvaluator(new ArgbEvaluator ());
			_warningAnimation.SetInterpolator(new LinearInterpolator());
			_warningAnimation.SetDuration(250); 
			_warningAnimation.RepeatMode = ValueAnimatorRepeatMode.Reverse;
			_warningAnimation.RepeatCount = -1;  //infinite
		}

		private void UpdateUI(View view)
		{
			var name = view.FindViewById<TextView>(Resource.Id.workoutName);
			name.Text = _currentWorkout.Name;

			var level = view.FindViewById<TextView>(Resource.Id.workoutLevel);
			level.Text = string.Format("{0} Intensity", _currentWorkout.Level);

			_timeRemainingInStageView = view.FindViewById<TextView>(Resource.Id.timeRemainingInStage);
			_elapsedTimeView = view.FindViewById<TextView>(Resource.Id.totalElapsed);
			_currentStageView = view.FindViewById<TextView>(Resource.Id.currentStage);
			_resistanceView = view.FindViewById<TextView>(Resource.Id.resistance);
			_crossrampView = view.FindViewById<TextView>(Resource.Id.crossRamp);
			_stridesView = view.FindViewById<TextView>(Resource.Id.strides);
			_remainingView = view.FindViewById<LinearLayout> (Resource.Id.remainingView);
			_startButton = view.FindViewById<Button> (Resource.Id.startButton);

			UpdateStage();

			var startButton = view.FindViewById<Button>(Resource.Id.startButton);
			if (startButton != null)
				startButton.Click += HandleStartClick;

			if (_workoutTimer.IsRunning)
				startButton.Visibility = ViewStates.Invisible;
		}

		/// <summary>
		/// Updates when the timer changes the current stage.
		/// </summary>
		private void UpdateStage()
		{
			//StopWarningAnimation();

			var currentStage = _workoutTimer.CurrentStage;

			this.Activity.RunOnUiThread(() => {			
				_resistanceView.TextFormatted = Html.FromHtml(currentStage.Resistance.ToString());
				_crossrampView.TextFormatted = Html.FromHtml(currentStage.CrossRamp.ToString());
			});

			UpdateStrides(currentStage);
			UpdateCurrentStageMessage();
			UpdateVisuals();
		}

		/// <summary>
		/// Updates on every tick of the timer.
		/// </summary>
		private void UpdateVisuals() 
		{
			UpdateTime();

			// Show toast if there's a message to display.
			if (!string.IsNullOrEmpty(_workoutTimer.CurrentStage.Message)) 
			{
				var t = Toast.MakeText(this.Activity, _workoutTimer.CurrentStage.Message, ToastLength.Short);
				t.Show();
			}
		}

		private void UpdateStrides(Stage stage)
		{
			var content = Html.FromHtml(stage.Strides.ToString());

			if (stage.Strides.Maximum == 999)
			{
				content = Html.FromHtml("Go <b>Max</b>!");
			}

			this.Activity.RunOnUiThread(() => {
				_stridesView.TextFormatted = content;
			});
		}

		private void UpdateCurrentStageMessage() 
		{
			var template = Resources.GetString(Resource.String.stage_template);

			this.Activity.RunOnUiThread(() => {
				_currentStageView.Text = string.Format(template, _workoutTimer.CurrentStageIndex + 1, _currentWorkout.Stages.Count);
			});
		}

		private void UpdateTime()
		{
			this.Activity.RunOnUiThread(() => {
				_timeRemainingInStageView.Text = _workoutTimer.RemainingTime.ToString(@"mm\:ss");
				_elapsedTimeView.Text = _workoutTimer.Elapsed.ToString(@"hh\:mm\:ss");
			});
		}

		private void HandleStartClick(object sender, EventArgs e)
		{
			var button = sender as Button;
			button.Visibility = ViewStates.Invisible;

			_workoutTimer.Start();
		}

		private void HandleTimerTick(WorkoutMessage.Status status)
		{
			switch (status)
			{
				case WorkoutMessage.Status.Warning:
					this.Activity.RunOnUiThread(()=> {
						var v = (Vibrator) this.Activity.GetSystemService(Activity.VibratorService);
						v.Vibrate(250);
					});

					StartWarningAnimation();
					UpdateVisuals();
					break;

				case WorkoutMessage.Status.Completed:
					_workoutTimer.Stop();
					StopWarningAnimation();

					this.Activity.RunOnUiThread(() => {
						_startButton.Text = Resources.GetString(Resource.String.end_button);
						_startButton.Visibility = ViewStates.Visible;
					});

					UpdateVisuals();
					break;

				case WorkoutMessage.Status.StageChanged:
					StopWarningAnimation();
					UpdateStage();
					break;

				case WorkoutMessage.Status.TimerTicked:
					UpdateVisuals();
					break;
			}
		}

		private void OnWorkoutTimerTick(object sender, WorkoutTimerTickEventArgs e) 
		{
			HandleTimerTick(e.Message.MessageStatus);
		}

		#region IEventSink implementation
		public void HandleEvent(WorkoutMessage publishedEvent)
		{
			HandleTimerTick(publishedEvent.MessageStatus);
		}
		#endregion
		
	}
}

