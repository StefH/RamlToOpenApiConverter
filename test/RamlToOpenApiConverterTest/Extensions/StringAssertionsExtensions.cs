//using System;
//using System.Collections.Generic;
//using System.Text;
//using FluentAssertions;
//using FluentAssertions.Primitives;

//namespace RamlToOpenApiConverterTest.Extensions
//{
//    internal static class StringAssertionsExtensions
//    {
//        public static AndConstraint<StringAssertions> BeEquivalentTo2(this StringAssertions sa, string expected)
//        {
//            return sa.Be() Normalize(sa.Subject) == Normalize(expected)
//        }

//        private static string Normalize(string x)
//        {
//            return x?.Replace("\r\n", "\n").Replace("\r", "\n");
//        }
//    }
//}