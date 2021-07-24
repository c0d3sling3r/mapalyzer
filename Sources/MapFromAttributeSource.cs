using System;
using System.Collections.Generic;
using System.Text;

using EasyMapper.Common;
using EasyMapper.Builders;
using EasyMapper.Sources.Abstracts;

namespace EasyMapper.Sources
{
    internal class MapFromAttributeSource : AAttributeSource
    {
        internal override string AttributeName => "MapFrom";

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
                    .WriteLine("/// Specifies mapping from the type provided as <see cref=\"SourceType\" />")
                    .WriteLine("/// </summary>")

                    .WriteLine("[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]")
                    .WriteLine("public sealed class {0}Attribute : Attribute", AttributeName)
                    .WriteOpeningBracket()

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Gets the source type of mapping")
                    .WriteLine("/// </summary>")

                    .WriteLine("public Type SourceType { get; }")

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Initializing a new instance of <see cref=\"{0}Attribute\" /> class which specifies source type as <paramref name=\"sourceType\" />", AttributeName)
                    .WriteLine("/// </summary>")

                    .WriteLine("public {0}Attribute(Type sourceType)", AttributeName)
                    .WriteOpeningBracket()
                    .WriteLine("SourceType = sourceType;")
                    .WriteClosingBracket()

                    .WriteClosingBracket() // class
                    .WriteClosingBracket(); // namespace
            }

            return sourceBuilder.ToString();
        }

        ~MapFromAttributeSource() { }
    }
}