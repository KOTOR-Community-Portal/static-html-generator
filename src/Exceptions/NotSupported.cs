using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticHtmlGenerator.Exceptions {
	public static class NotSupported {
		public static void ThrowIfReadOnly([DoesNotReturnIf(true)] bool isReadOnly) {
			if( isReadOnly )
				throw new NotSupportedException("The specified operation is not supported by this read-only collection.");
		}
	}
}