using System.Collections.Generic;

namespace RamlToOpenApiConverter.Yaml
{
    public class IncludeRefCallback
    {
        private readonly IDictionary<object, object> _types;

        public IncludeRefCallback(IDictionary<object, object> types)
        {
            _types = types;
        }

        public void Add(string name, IDictionary<object, object> values)
        {
            _types.Add(name, values);
        }
    }
}