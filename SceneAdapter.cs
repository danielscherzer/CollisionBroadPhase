using System;
using System.Collections.Generic;
using Zenseless.Patterns;

namespace Example
{
	public class SceneAdapter : NotifyPropertyChanged, IGameObjectProvider
	{
		public SceneAdapter(int objectCount, float objectMinSize, float objectSizeVariation)
		{
			_freeze = false;
			_objectCount = objectCount;
			_objectMinSize = objectMinSize;
			_objectSizeVariation = objectSizeVariation;
			Regenerate();
		}

		public bool Freeze
		{
			get => _freeze;
			set => SetNotify(ref _freeze, value);
		}

		[UiIgnore]
		public IReadOnlyList<GameObject> GameObjects => _immutableScene.GameObjects;

		[UiIncrement(1000)]
		public int ObjectCount
		{
			get => _objectCount;
			set => SetNotify(ref _objectCount, value, _ => Regenerate());
		}

		[UiIncrement(0.001f)]
		public float ObjectMinSize
		{
			get => _objectMinSize;
			set => SetNotify(ref _objectMinSize, value, _ => Regenerate());
		}

		[UiIncrement(0.001f)]
		public float ObjectSizeVariation
		{
			get => _objectSizeVariation;
			set => SetNotify(ref _objectSizeVariation, value, _ => Regenerate());
		}

		public event EventHandler OnRegeneration;

		public void Update(float frameTime)
		{
			frameTime = Freeze ? 0f : frameTime;
			_immutableScene.Update(frameTime);
		}

		private bool _freeze;
		private int _objectCount;
		private float _objectMinSize;
		private float _objectSizeVariation;
		private Scene _immutableScene;

		private void Regenerate()
		{
			_immutableScene = new Scene(ObjectCount, ObjectMinSize, ObjectSizeVariation);
			OnRegeneration?.Invoke(this, EventArgs.Empty);
		}
	}
}
