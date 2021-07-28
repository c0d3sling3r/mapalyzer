using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using Mapalyzer.Common;
using Mapalyzer.Data;
using Mapalyzer.Builders;

namespace Mapalyzer.Sources
{
    internal static class MappedClassSource
    {
        static List<Diagnostic> _diagnostics;
        static string _sourceTypeCamelCaseName;
        static string _destinationTypeCamelCaseName;

        private static string ProvideSource(MappingMetaData mappingMetaData, Compilation compilation)
        {
            SourceBuilder sourceBuilder = new();
            
            if (mappingMetaData.PropertyMapHolders.Any())
            {                
                _destinationTypeCamelCaseName = mappingMetaData.DestinationTypeName.ToCamelCase();
                _sourceTypeCamelCaseName = mappingMetaData.SourceTypeName.ToCamelCase();
                
                string sourceNameSpace = mappingMetaData.InjectableNameSpaces.Single(ins => ins.ContainsSourceClass).NameSpace;
                string classCtorArguments = string.Join(", ", ProvidePropertiesWithType(mappingMetaData.PropertyMapHolders));

                using (sourceBuilder)
                {
                    sourceBuilder.WriteLine(Constants.GeneratedFileHeader)

                        // Writes nameSpaces
                        .WriteLine("using System;")
                        .WriteLine("using System.Collections.Generic;")
                        .WriteLine("using System.Linq;")
                        .WriteClassNameSpaces(mappingMetaData.InjectableNameSpaces.Where(ins => !ins.ContainsSourceClass).Select(ins => ins.NameSpace).ToList())
                        .WriteLine()

                        // Writes class namespace
                        .WriteLine("namespace {0}", mappingMetaData.DestinationNamespace)
                        .WriteOpeningBracket()

                        // Writes class
                        .WriteLine("public partial class {0}", mappingMetaData.DestinationTypeName)
                        .WriteOpeningBracket()
                        .WriteLine("public {0}({1})", mappingMetaData.DestinationTypeName, classCtorArguments)
                        .WriteOpeningBracket()
                        .WriteConstructorPropertiesInitializations(mappingMetaData.PropertyMapHolders)
                        .WriteClosingBracket()
                        .WriteClosingBracket() // class
                        .WriteClosingBracket() // class namespace
                        .WriteLine()

                        // Writes extension namespace
                        .WriteLine("namespace {0}", compilation.AssemblyName)
                        .WriteOpeningBracket()

                        // Writes fromTo extension
                        .WriteLine("public static partial class {0}Extensions", mappingMetaData.DestinationTypeName)
                        .WriteOpeningBracket()
                        .WriteLine()
                        .WriteLine("public static {0}.{1} To{1}(this {2}.{3} source_{4}, Action<{2}.{3}, {0}.{1}> postMappingAction = null)", 
                            mappingMetaData.DestinationNamespace,
                            mappingMetaData.DestinationTypeName, 
                            sourceNameSpace,
                            mappingMetaData.SourceTypeName, 
                            _sourceTypeCamelCaseName)

                        .WriteOpeningBracket()
                        .WriteLine("if (source_{0} is null)", _sourceTypeCamelCaseName)
                        .WriteIndentedLine("throw new ArgumentNullException(nameof(source_{0}));", _sourceTypeCamelCaseName)
                        .WriteLine()
                        .WriteConverterReturnExpression(mappingMetaData, compilation)
                        .WriteClosingBracket()
                        .WriteLine()

                        // Writes fromToList extension
                        .WriteLine("#nullable enable")
                        .WriteLine("public static IEnumerable<{0}.{1}>? To{2}(this IEnumerable<{3}.{4}> source_{5}, Action<{3}.{4}, {0}.{1}>? postMappingAction = null)", 
                            mappingMetaData.DestinationNamespace,
                            mappingMetaData.DestinationTypeName, 
                            mappingMetaData.DestinationTypeName.Pluralize(), 
                            sourceNameSpace,
                            mappingMetaData.SourceTypeName, 
                            _sourceTypeCamelCaseName.Pluralize())

                        .WriteOpeningBracket()
                        .WriteLine("return source_{0}?.Select(i => i.To{1}(postMappingAction));", _sourceTypeCamelCaseName.Pluralize(), mappingMetaData.DestinationTypeName)
                        .WriteClosingBracket()
                        .WriteLine("#nullable disable")

                        .WriteClosingBracket() // extension class
                        .WriteClosingBracket(); // extension namespace
                }
            }

            return sourceBuilder.ToString();
        }

        #region Extensions Additions

        private static SourceBuilder WriteClassNameSpaces(this SourceBuilder sourceBuilder, List<string> nameSpaces)
        {
            if (nameSpaces.Any())
                nameSpaces.ForEach(ns => sourceBuilder.WriteLine("using {0};", ns));

            return sourceBuilder;
        }

        private static SourceBuilder WriteConstructorPropertiesInitializations(this SourceBuilder sourceBuilder, List<PropertyMapHolder> propertyMapHolders)
        {
            propertyMapHolders.ForEach(p => sourceBuilder.WriteLine("{0} = {1};", p.DestinationPropertySymbol.Name, p.DestinationPropertySymbol.Name.ToCamelCase()));
            return sourceBuilder;
        }

        private static SourceBuilder WriteConverterReturnExpression(this SourceBuilder sourceBuilder, MappingMetaData mappingMetaData, Compilation compilation)
        {
            string line = string.Empty;
            sourceBuilder.Write("{0}.{1} {2} = new {0}.{1}(", mappingMetaData.DestinationNamespace, mappingMetaData.DestinationTypeName, mappingMetaData.DestinationTypeName.ToCamelCase());

            foreach (PropertyMapHolder pmh in mappingMetaData.PropertyMapHolders)
            {
                if (pmh.SourcePropertySymbol is not null)
                {
                    if (pmh.ForceConversion)
                        line = string.Format("Convert.To{2}(source_{0}.{1})", _sourceTypeCamelCaseName, pmh.SourcePropertySymbol.Name, pmh.DestinationPropertySymbol.Type.Name);
                    else
                        line = string.Format("source_{0}.{1}", _sourceTypeCamelCaseName, pmh.SourcePropertySymbol.Name);
                }

                if (!string.IsNullOrEmpty(line))
                {
                    if (mappingMetaData.PropertyMapHolders.ToList().IndexOf(pmh) > 0)
                        sourceBuilder.WriteLine(",");

                    sourceBuilder.Write(line);
                }
            }

            sourceBuilder.WriteLine(");").WriteLine();

            sourceBuilder.WriteLine("if (postMappingAction is not null)")
                .WriteIndentedLine("postMappingAction(source_{1}, {0});", _destinationTypeCamelCaseName, _sourceTypeCamelCaseName)
                .WriteLine()
                .WriteLine("return {0};", _destinationTypeCamelCaseName);

            return sourceBuilder;
        }

        #endregion

        private static IEnumerable<string> ProvidePropertiesWithType(List<PropertyMapHolder> propertyMapHolders) =>
            propertyMapHolders.Select(pmh => string.Format("{0} {1}", pmh.DestinationPropertySymbol.Type.ToDisplayString(), pmh.DestinationPropertySymbol.Name.ToCamelCase()));

        internal static SourceCode Build(MappingMetaData mmd, Compilation cmp, List<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
            return new(ProvideSource(mmd, cmp), $"Map{mmd.SourceTypeName}To{mmd.DestinationTypeName}.generated.cs");
        }
    }
}
