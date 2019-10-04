using System;
using System.Collections.Generic;
using Zenseless.Patterns;

namespace Example
{
	public class SceneAdapter : NotifyPropertyChanged, IColliderProvider
	{
		public SceneAdapter(int objectCount, float objectMinSize, float objectSizeVariation)
		{
			_freeze = false;
			_objectCount = objectCount;
			_objectMinSize = objectMinSize;
			_objectSizeVariation = objectSizeVariation;
			Recreate();
		}

		public bool Freeze
		{
			get => _freeze;
			set => SetNotify(ref _freeze, value);
		}

		[UiIgnore]
		public IReadOnlyList<ICollider> Collider => _immutableScene.GameObjects;


		[UiIncrement(1000)]
		public int ObjectCount
		{
			get => _objectCount;
			set => SetNotify(ref _objectCount, value, _ => Recreate());
		}

		[UiIncrement(0.001f)]
		public float ObjectMinSize
		{
			get => _objectMinSize;
			set => SetNotify(ref _objectMinSize, value, _ => Recreate());
		}

		[UiIncrement(0.003f)]
		public float ObjectSizeVariation
		{
			get => _objectSizeVariation;
			set => SetNotify(ref _objectSizeVariation, value, _ => Recreate());
		}

		public event EventHandler OnChange;

		public void Update(float frameTime)
		{
			frameTime = Freeze ? 0f : frameTime;
			if (frameTime > 1f / 10f) frameTime = 1f / 20f;
			_immutableScene.Update(frameTime);
		}

		private bool _freeze;
		private int _objectCount;
		private float _objectMinSize;
		private float _objectSizeVariation;
		private Scene _immutableScene;

		private void Recreate()
		{
			_immutableScene = new Scene(ObjectCount, ObjectMinSize, ObjectSizeVariation);
			OnChange?.Invoke(this, EventArgs.Empty);
		}
	}
}
