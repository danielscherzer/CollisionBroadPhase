using System;
using System.Collections.Generic;
using System.Numerics;
using Example.UI;
using Zenseless.Patterns;

namespace Example
{
	public class SceneAdapter : NotifyPropertyChanged, IColliderProvider
	{
		public SceneAdapter(int objectCount, float objectMinSize, float objectSizeVariation)
		{
			_freeze = true;
			_movingObjectPercentage = 0f;
			_objectCount = objectCount;
			_objectMinSize = objectMinSize;
			_objectSizeVariation = objectSizeVariation;
			Recreate();
			MovingObjectPercentage = 0.1f;
		}

		[UiValueChangeFunction(0, 2)]
		public float MovingObjectPercentage
		{
			get => _movingObjectPercentage;
			set => Set(ref _movingObjectPercentage, value, SetMovingObjectPercentage);
		}

		public bool Freeze
		{
			get => _freeze;
			set => Set(ref _freeze, value);
		}

		[UiIgnore]
		public IReadOnlyList<ICollider> Collider => _immutableScene.GameObjects;


		[UiValueChangeFunction(0, 2)]
		public int ObjectCount
		{
			get => _objectCount;
			set => Set(ref _objectCount, value, _ => Recreate());
		}

		[UiValueChangeFunction(0, 1.1)]
		public float ObjectMinSize
		{
			get => _objectMinSize;
			set => Set(ref _objectMinSize, value, _ => Recreate());
		}

		[UiValueChangeFunction(0, 1.1)]
		public float ObjectSizeVariation
		{
			get => _objectSizeVariation;
			set => Set(ref _objectSizeVariation, value, _ => Recreate());
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
		private float _movingObjectPercentage;

		private void SetMovingObjectPercentage(float percentage)
		{
			_movingObjectPercentage = MathF.Min(1f, MathF.Max(MathF.Round(percentage, 3), 0f));
			var movingObjects = (int)(_movingObjectPercentage * ObjectCount);
			foreach(var gameObject in _immutableScene.GameObjects)
			{
				gameObject.Velocity = (movingObjects >= 0) ? _immutableScene.RandomVelocity : Vector2.Zero;
				--movingObjects;
			}
		}

		private void Recreate()
		{
			_immutableScene = new Scene(ObjectCount, ObjectMinSize, ObjectSizeVariation);
			SetMovingObjectPercentage(MovingObjectPercentage);
			OnChange?.Invoke(this, EventArgs.Empty);
		}
	}
}
