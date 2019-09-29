using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zenseless.Patterns;

namespace Example
{
	class CollisionParameters : NotifyPropertyChanged, ICollisionParameters
	{
		[UiIncrement(8)]
		public int CellCount
		{
			get => _cellCount;
			set => SetNotify(ref _cellCount, value);
		}

		public bool CollisionDetection
		{
			get => _collisionDetection;
			set => SetNotify(ref _collisionDetection, value);
		}

		public CollisionMethodTypes CollisionMethod
		{
			get => _collisionMethod;
			set => SetNotify(ref _collisionMethod, value);
		}

		public bool DebugAlgo
		{
			get => _debugAlgo;
			set => SetNotify(ref _debugAlgo, value);
		}

		protected new void SetNotify<TYPE>(ref TYPE valueBackend, TYPE value, Action<TYPE> action = null, [CallerMemberName] string memberName = "")
		{
			if (EqualityComparer<TYPE>.Default.Equals(valueBackend, value)) return;
			base.SetNotify(ref valueBackend, value, action, memberName);
		}

		private bool _collisionDetection = true;
		private int _cellCount = 32;
		private CollisionMethodTypes _collisionMethod = CollisionMethodTypes.PersistentSAP;
		private bool _debugAlgo = false;
	}
}
