using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Example
{
	public static class Property
	{
		public static void OnChange<T>(this INotifyPropertyChanged instance, string propertyName, Action<T> action, bool immediateAction = true)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentException($"Property {propertyName} not found.");
			void ExecuteAction()
			{
				var value = (T)property.GetValue(instance);
				action(value);
			}
			instance.PropertyChanged += (_, a) =>
			{
				if (a.PropertyName == propertyName)
				{
					ExecuteAction();
				}
			};
			if (immediateAction) ExecuteAction();
		}

		public static void OnChange<T>(Expression<Func<T>> propertyExpression, Action<T> action, bool immediateAction = true)
		{
			var member = (MemberExpression)propertyExpression.Body;
			var instance = Expression.Lambda(((MemberExpression)member.Expression)).Compile().DynamicInvoke();
			var propertyName = member.Member.Name;
			OnChange((INotifyPropertyChanged)instance, propertyName, action);
		}

		public static void Bind<T>(Expression<Func<T>> equalLambda)
		{
			var equal = (BinaryExpression)equalLambda.Body;

			var left = (MemberExpression)equal.Left;
			var leftInstance = Expression.Lambda(((MemberExpression)left.Expression)).Compile().DynamicInvoke();
			var leftPropertyName = left.Member.Name;

			var right = (MemberExpression)equal.Right;
			var rightInstance = Expression.Lambda(((MemberExpression)right.Expression)).Compile().DynamicInvoke();
			var rightPropertyName = right.Member.Name;

			var assignment = Expression.Lambda(Expression.Assign(left, right)).Compile();

			var notify = (INotifyPropertyChanged)rightInstance;
			notify.PropertyChanged += (_, a) =>
			{
				if (a.PropertyName == rightPropertyName)
				{
					assignment.DynamicInvoke();
				}
			};
		}
	}
}
