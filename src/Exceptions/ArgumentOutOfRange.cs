using System;

namespace StaticHtmlGenerator.Exceptions {
	public static class ArgumentOutOfRange {
		public static void Throw(string message, string?[] paramNames) {
			throw new ArgumentOutOfRangeException(Argument.AddParameters(message, paramNames));
		}

		public static void ThrowIfCapacityNegative(int value, string? paramName = null) {
			if( value < 0 )
				throw new ArgumentOutOfRangeException(paramName, "Capacity cannot be negative.");
		}

		public static void ThrowIfIndexOutOfRange(int value, int length, string? paramName = null) {
			if( value < 0 || value >= length )
				throw new ArgumentOutOfRangeException(paramName, $"Index must be in the range [0,{length}).");
		}

		public static void ThrowIfIndexOutOfRange(int value, int length, string rangeName, string? paramName = null) {
			if( value < 0 || value >= length )
				throw new ArgumentOutOfRangeException(paramName, value, $"{rangeName} index must be in the range [0,{length}).");
		}

		public static void ThrowIfLengthNegative(int value, string? paramName = null) {
			if( value < 0 )
				throw new ArgumentOutOfRangeException(paramName, "Length cannot be negative.");
		}

		public static void ThrowIfLengthNotEqual(int actualValue, int expectedValue, string? paramName = null) {
			if( actualValue != expectedValue )
				throw new ArgumentOutOfRangeException(paramName, actualValue, $"Length must be {expectedValue}.");
		}

		public static void ThrowIfLengthNotEqual(int actualValue, int expectedValue, string rangeName, string? paramName = null) {
			if( actualValue != expectedValue )
				throw new ArgumentOutOfRangeException(paramName, actualValue, $"{rangeName} length must be {expectedValue}.");
		}
	}
}