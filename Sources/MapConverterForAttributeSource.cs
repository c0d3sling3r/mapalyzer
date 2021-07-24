using System;
using System.Collections.Generic;
using System.Text;

using EasyMapper.Common;
using EasyMapper.Builders;
using EasyMapper.Sources.Abstracts;

namespace EasyMapper.Sources
{
    internal class MapConverterForAttributeSource : AAttributeSource
    {
        internal override string AttributeName => "MapConverterFor";

        internal override string ProvideSource()
        {
             SourceBuilder sourceBuilder = new();
            using (sourceBuilder)
            {
                sourceBuilder.WriteLine(Constants.GeneratedFileHeader)
                    .WriteLine("using System;")
                    .WriteLine()

                    .WriteLine("namespace System")
                    .WriteOpeningBracket()

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Specifies a converter for map the field from source corresponding field which is specified as <see cref=\"DestinationPropertyName\"/>")
                    .WriteLine("/// </summary>")

                    .WriteLine("[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]")
                    .WriteLine("public sealed class {0}Attribute : Attribute", AttributeName)
                    .WriteOpeningBracket()

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Gets the converter")
                    .WriteLine("/// </summary>")

                    .WriteLine("public string DestinationPropertyName { get; }")

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Initializing a new instance of <see cref=\"{0}Attribute\" /> class which specifies converter type as <paramref name=\"destinationPropertyName\"/>", AttributeName)
                    .WriteLine("/// </summary>")

                    .WriteLine("public {0}Attribute(string destinationPropertyName)", AttributeName)
                    .WriteOpeningBracket()
                    .WriteLine("DestinationPropertyName = destinationPropertyName;")
                    .WriteClosingBracket()

                    .WriteClosingBracket() // class
                    .WriteClosingBracket(); // namespace
            }

            return sourceBuilder.ToString();
        }

        ~MapConverterForAttributeSource() { }
    }
}