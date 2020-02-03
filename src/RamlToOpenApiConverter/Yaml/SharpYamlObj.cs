using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;


namespace RamlToOpenApiConverter.Yaml
{
    public class SharpYamlObj : ObjectSerializer
    {
        private object p;

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

                return "hello";
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
