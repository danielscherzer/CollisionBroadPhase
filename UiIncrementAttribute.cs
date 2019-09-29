using System;

namespace Example
{
	public class UiIncrementAttribute : Attribute
	{
		public UiIncrementAttribute(double value)
		{
			Value = value;
		}

		public double Value { get; }
	}
}
