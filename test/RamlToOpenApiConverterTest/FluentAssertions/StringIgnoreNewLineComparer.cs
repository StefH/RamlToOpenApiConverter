using System;
using System.Collections.Generic;
using System.Text;

namespace RamlToOpenApiConverterTest.FluentAssertions
{
    internal class StringIgnoreNewLineComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return Normalize(x) == Normalize(y);
        }

        private static string Normalize(string x)
        {
            return x?.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        public int GetHashCode(string obj) => obj.GetHashCode();
    }
}