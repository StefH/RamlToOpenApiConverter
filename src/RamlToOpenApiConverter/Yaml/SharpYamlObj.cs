using System.Collections.Generic;
using System.IO;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;


namespace RamlToOpenApiConverter.Yaml
{
    public class SharpYamlObj : ObjectSerializer
    {
        private readonly string _directoryName;
        private readonly InlineTypeCallback _inlineTypeCallback;
        private object p;

        public SharpYamlObj(string directoryName, InlineTypeCallback inlineTypeCallback)
        {
            _directoryName = directoryName;
            _inlineTypeCallback = inlineTypeCallback;
        }

        public override object ReadYaml(ref ObjectContext objectContext)
        {
            //var parsingEvent = objectContext.Reader.Peek<ParsingEvent>();
            //// Can this happen here?
            //if (parsingEvent == null)
            //{
            //    // TODO check how to put a location in this case?
            //    throw new YamlException("Unable to parse input");
            //}

            //var node = parsingEvent as NodeEvent;
            //if (node == null)
            //{
            //    throw new YamlException(parsingEvent.Start, parsingEvent.End, "?");
            //}


            var peek = objectContext.Reader.Peek<Scalar>();
            if (peek != null && peek.Tag == "!include")
            {

                string fileName = peek.Value;
                string includePath = Path.Combine(_directoryName, peek.Value);
                var includeText = File.ReadAllText(includePath);

                var value = objectContext.SerializerContext.ReadYaml(includeText, typeof(IDictionary<object, object>));

                _inlineTypeCallback.Add(Path.GetFileNameWithoutExtension(fileName), value as IDictionary<object, object>);

                return value;
            }

            //var scalar = objectContext.Reader.Expect<Scalar>();

            //if (scalar != null && scalar.Tag == "!include")
            //{
            //    return "hello";
            //}

            p = base.ReadYaml(ref objectContext);

            return p;
        }
    }
}
