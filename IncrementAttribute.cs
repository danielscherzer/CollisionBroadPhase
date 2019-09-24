using System;

namespace Example
{
	public class IncrementAttribute : Attribute
	{
		public IncrementAttribute(double value)
		{
			Value = value;
		}

		public double Value { get; }
	}
}
