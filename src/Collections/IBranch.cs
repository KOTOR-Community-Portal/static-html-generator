using System;
using System.Collections.Generic;

namespace StaticHtmlGenerator.Collections
{
    public interface IBranch<T> : IList<T>, IReadOnlyList<T>, IEquatable<IBranch<T>>
    {
        new T this[int index] { get; set; }
        new int Count { get; }
    }
}