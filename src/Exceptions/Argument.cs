using StaticHtmlGenerator.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace StaticHtmlGenerator.Exceptions {
	public static class Argument {
		public static string AddParameters(string message, params string?[] paramNames) {
			if( paramNames.Length < 1 || string.IsNullOrEmpty(paramNames[0]) ) {
				return message;
			}
			else {
				var sb = new StringBuilder(message).Append(" (Parameter");
				if( sb.Length > 1 ) {
					sb.Append("s ");
					for( int i = 0; i < paramNames.Length - 1; ++i )
						sb.Append('\'').Append(paramNames[i]).Append("', ");
				}
				sb.Append('\'').Append(paramNames[^1]).Append('\'');
				sb.Append(')');
				return sb.ToString();
			}
		}

		[DoesNotReturn]
		public static void Throw(string message, params string?[] paramNames) {
			throw new ArgumentException(AddParameters(message, paramNames));
		}

		public static void ThrowIfContainsNull<T>(IEnumerable<T> collection, string? paramName = null) {
			foreach( T item in collection )
				if( item == null )
					throw new ArgumentException("The specified collection contains a null element.", paramName);
		}

		public static void ThrowIfCountLessThan(int actualValue, int minValue, string? paramName = null) {
			if( actualValue < minValue )
				throw new ArgumentException($"Count must be at least {minValue}.", paramName);
		}

		public static void ThrowIfCountNotEqual(int actualValue, int expectedValue, string? paramName = null) {
			if( actualValue != expectedValue )
				throw new ArgumentException($"Count must be {expectedValue}.", paramName);
		}

		[Discardable]
		[return: MaybeNull]

		[return: NotNullIfNotNull(nameof(value))]
		public static T ThrowIfNotOfType<T>(object? value, string? paramName = null) {
			if( !(value is T || (value == null && default(T) == null && Nullable.GetUnderlyingType(typeof(T)) != null)) )
				throw new ArgumentException($"Value \"{value}\" is not of type \"{typeof(T)}\".", paramName);
			return (T)value!;
		}
	}
}