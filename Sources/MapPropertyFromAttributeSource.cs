using System;
using System.Collections.Generic;
using System.Text;

using EasyMapper.Common;
using EasyMapper.Builders;
using EasyMapper.Sources.Abstracts;

namespace EasyMapper.Sources
{
    internal class MapPropertyFromAttributeSource : AAttributeSource
    {
        internal override string AttributeName => "MapPropertyFrom";

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
                    .WriteLine("/// Specifies mapping from the property provided as <see cref=\"SourcePropertyName\"/>.")
                    .WriteLine("/// </summary>")

                    .WriteLine("/// <remarks>")
                    .WriteLine("/// If the desired properties have same name but their types are different, you can force conversion by setting <see cref=\"IgnoreTypeDifference\"/> property as true.")
                    .WriteLine("/// </remarks>")

                    .WriteLine("[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]")
                    .WriteLine("public sealed class {0}Attribute : Attribute", AttributeName)
                    .WriteOpeningBracket()

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Gets the source property name of mapping")
                    .WriteLine("/// </summary>")
                    .WriteLine("public string SourcePropertyName { get; }")
                    .WriteLine()
                    
                    .WriteLine("/// <summary>")
                    .WriteLine("/// Forces the conversion by changing source type.")
                    .WriteLine("/// </summary>")
                    .WriteLine("public bool IgnoreTypeDifference { get; }")
                    .WriteLine()

                    .WriteLine("/// <summary>")
                    .WriteLine("/// Initializing a new instance of <see cref=\"{0}Attribute\" /> class which specifies source type as <paramref name=\"sourcePropertyName\"/>", AttributeName)
                    .WriteLine("/// </summary>")
                    .WriteLine("/// <param name=\"sourcePropertyName\">Used to get source property.</param>")
                    .WriteLine("/// <param name=\"ignoreTypeDifference\">If set as true, mapping will be done regardless of type difference.</param>")

                    .WriteLine("public {0}Attribute(string sourcePropertyName, bool ignoreTypeDifference = false)", AttributeName)
                    .WriteOpeningBracket()
                    .WriteLine("SourcePropertyName = sourcePropertyName;")
                    .WriteLine("IgnoreTypeDifference = ignoreTypeDifference;")
                    .WriteClosingBracket()

                    .WriteClosingBracket() // class
                    .WriteClosingBracket(); // namespace
            }

            return sourceBuilder.ToString();
        }

        ~MapPropertyFromAttributeSource() { }
    }
}