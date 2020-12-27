using System;

namespace UI
{
	public sealed class UiValueChangeFunctionAttribute : Attribute
	{
		public UiValueChangeFunctionAttribute(double constant = 1.0, double multiplier = 1.0)
		{
			Multiplier = multiplier;
			Constant = constant;
		}

		public UiValueChangeFunctionAttribute Inverted() => new UiValueChangeFunctionAttribute(-Constant, 1.0 / Multiplier);

		public double NewValue(double value) => Multiplier * value + Constant;

		public double Multiplier { get; }
		public double Constant { get; }
	}
}
