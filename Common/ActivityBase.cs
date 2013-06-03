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
	public class ActivityBase : Activity
	{
		private Stack<Fragment> _fragments = new Stack<Fragment>();
		private int _fragmentContainerId;

		public ActivityBase(int fragmentContainerId)
		{
			_fragmentContainerId = fragmentContainerId;
		}

		protected void SwitchContent(Fragment fragment) 
		{
			var ftx = FragmentManager.BeginTransaction();

			ftx.Replace(_fragmentContainerId, fragment);

			// Do animation

			ftx.Commit();
		}

		public void PushFragment(Fragment fragment) 
		{
			_fragments.Push(fragment);
			SwitchContent(fragment);
		}

		public bool PopFragment()
		{
			if (_fragments.Count <= 1)
				return false;

			var fragment = _fragments.Pop();
			fragment = _fragments.Peek();
			SwitchContent(fragment);

			return true;
		}

		// If Back button is pressed, return previous fragment.
		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back) {
				return PopFragment();
			}

			return base.OnKeyDown(keyCode, e);
		}
	}
}

