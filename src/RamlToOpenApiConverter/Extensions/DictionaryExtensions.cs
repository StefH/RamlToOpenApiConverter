using System;
using System.Collections.Generic;

namespace RamlToOpenApiConverter.Extensions
{
    internal static class DictionaryExtensions
    {
        public static string? Get(this IDictionary<object, object> source, object key)
        {
            if (!source.ContainsKey(key))
            {
                return null;
            }

            return source[key] as string;
        }

        public static T Get<T>(this IDictionary<object, object> source, object key)
        {
            if (!source.ContainsKey(key))
            {
                return default!;
            }

            return ChangeTypeEx<T>(source[key]);
        }

        public static IDictionary<object, object>? GetAsDictionary(this IDictionary<object, object> source, object key)
        {
            if (source.TryGetValue(key, out object value))
            {
                return value as IDictionary<object, object>;
            }

            return null;
        }

        public static void Replace(this IDictionary<object, object> source, IDictionary<object, object> newValue, object key)
        {
            if (source.TryGetValue(key, out _))
            {
                if (newValue.Count > 0)
                {
                    source.Remove(key);
                }

                foreach (var item in newValue)
                {
                    source.Add(item.Key, item.Value);
                }
            }
        }

        public static ICollection<object>? GetAsCollection(this IDictionary<object, object> source, object key)
        {
            if (source.TryGetValue(key, out object value))
            {
                return value as ICollection<object>;
            }

            return null;
        }

        public static string? GetAsString(this IDictionary<object, object> source, object key)
        {
            if (source.TryGetValue(key, out var value))
            {
                return value is string ? value.ToString() : null;
            }

            return null;
        }

        private static T ChangeTypeEx<T>(object obj)
        {
            Type type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // get the T in ?T
                var typeArgument = type.GetGenericArguments()[0];
                obj = Convert.ChangeType(obj, typeArgument);

                // get the Nullable<T>(T) constructor
                var ctor = type.GetConstructor(new[] { typeArgument })!;
                return (T)ctor.Invoke(new[] { obj });
            }

            return (T)Convert.ChangeType(obj, type);
        }
    }
}