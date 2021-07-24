using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using EasyMapper.Common;
using EasyMapper.Data;
using EasyMapper.Builders;

namespace EasyMapper.Sources
{
    internal static class MappedClassSource
    {
        static string _sourceTypeCamelCaseName;
        static string _destinationTypeCamelCaseName;

        private static string ProvideSource(MappingMetaData mappingMetaData, Compilation compilation)
        {
            SourceBuilder sourceBuilder = new();
            
            if (mappingMetaData.PropertyMapHolders.Any())
            {
                string sourceNameSpace = mappingMetaData.InjectableNameSpaces.Single(ins => ins.ContainsSourceClass).NameSpace;
                string classCtorArguments = string.Join(", ", ProvidePropertiesWithType(mappingMetaData.PropertyMapHolders));
                
                _destinationTypeCamelCaseName = mappingMetaData.DestinationTypeName.ToCamelCase();
                _sourceTypeCamelCaseName = mappingMetaData.SourceTypeName.ToCamelCase();

                using (sourceBuilder)
                {
                    sourceBuilder.WriteLine(Constants.GeneratedFileHeader)

                        //Writes nameSpaces
                        .WriteLine("using System;")
                        .WriteLine("using System.Collections.Generic;")
                        .WriteLine("using System.Linq;")
                        .WriteClassNameSpaces(mappingMetaData.InjectableNameSpaces.Select(ins => ins.NameSpace).ToList())
                        .WriteLine()

                        // Writes namespaces
                        .WriteLine("namespace {0}", mappingMetaData.Namespace)
                        .WriteOpeningBracket()

                        // Writes class
                        .WriteLine("public partial class {0}", mappingMetaData.DestinationTypeName)
                        .WriteOpeningBracket()
                        .WriteLine("public {0}({1})", mappingMetaData.DestinationTypeName, classCtorArguments)
                        .WriteOpeningBracket()
                        .WriteConstructorPropertiesInitializations(mappingMetaData.PropertyMapHolders)
                        .WriteClosingBracket()
                        .WriteClosingBracket() // class
                        .WriteLine()

                        // Writes fromTo extension
                        .WriteLine("public static partial class {0}Extensions", mappingMetaData.DestinationTypeName)
                        .WriteOpeningBracket()
                        .WriteLine("#nullable enable")
                        .WriteLine()
                        .WriteLine("public static {0} To{0}(this {1} {2}, Action<{1}, {0}> postMappingAction = null)", 
                            mappingMetaData.DestinationTypeName, 
                            mappingMetaData.SourceTypeName, 
                            _sourceTypeCamelCaseName)

                        .WriteOpeningBracket()
                        .WriteLine("if ({0} is null)", _sourceTypeCamelCaseName)
                        .WriteIndentedLine("throw new ArgumentNullException(nameof({0}));", _sourceTypeCamelCaseName)
                        .WriteLine()
                        .WriteConverterReturnExpression(mappingMetaData, compilation)
                        .WriteClosingBracket()
                        .WriteLine()

                        // Writes fromToList extension
                        .WriteLine("public static IEnumerable<{0}>? To{1}(this IEnumerable<{2}> {3}, Action<{2}, {0}> postMappingAction = null)", 
                            mappingMetaData.DestinationTypeName, 
                            mappingMetaData.DestinationTypeName.Pluralize(), 
                            mappingMetaData.SourceTypeName, 
                            _sourceTypeCamelCaseName.Pluralize())

                        .WriteOpeningBracket()
                        .WriteLine("return {0}?.Select(i => i.To{1}(postMappingAction));", _sourceTypeCamelCaseName.Pluralize(), mappingMetaData.DestinationTypeName)
                        .WriteClosingBracket()

                        .WriteClosingBracket() // extension class
                        .WriteClosingBracket(); // namespace
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

        private static SourceBuilder WriteConstructorPropertiesInitializations(this SourceBuilder sourceBuilder, IEnumerable<PropertyMapHolder> propertyMapHolders)
        {
            ((List<PropertyMapHolder>)propertyMapHolders).ForEach(p => sourceBuilder.WriteLine("{0} = {1};", p.DestinationPropertySymbol.Name, p.DestinationPropertySymbol.Name.ToCamelCase()));
            return sourceBuilder;
        }

        private static SourceBuilder WriteConverterReturnExpression(this SourceBuilder sourceBuilder, MappingMetaData mappingMetaData, Compilation compilation)
        {
            sourceBuilder.Write("{0} {1} = new {0}(", mappingMetaData.DestinationTypeName, mappingMetaData.DestinationTypeName.ToCamelCase());

            foreach (PropertyMapHolder pmh in mappingMetaData.PropertyMapHolders)
            {
                if (pmh.ConverterFieldSymbol is not null)
                    sourceBuilder.WriteIndented("{0}.{1}({2})", mappingMetaData.DestinationTypeName, pmh.ConverterFieldSymbol.Name, _sourceTypeCamelCaseName);
                else if (pmh.SourcePropertySymbol is not null)
                {
                    if (pmh.DestinationTypeSymbol is not null)
                        sourceBuilder.WriteIndented("Convert.To{2}({0}.{1})", _sourceTypeCamelCaseName, pmh.SourcePropertySymbol.Name, pmh.DestinationTypeSymbol.Name);
                    else
                        sourceBuilder.WriteIndented("{0}.{1}", _sourceTypeCamelCaseName, pmh.SourcePropertySymbol.Name);
                }

                if (mappingMetaData.PropertyMapHolders.ToList().IndexOf(pmh) != mappingMetaData.PropertyMapHolders.Count() - 1)
                    sourceBuilder.WriteLine(",");
            }

            sourceBuilder.WriteLine(");").WriteLine();

            sourceBuilder.WriteLine("if (postMappingAction is not null)")
                .WriteIndentedLine("postMappingAction({1}, {0});", _destinationTypeCamelCaseName, _sourceTypeCamelCaseName)
                .WriteLine()
                .WriteLine("return {0};", _destinationTypeCamelCaseName);

            return sourceBuilder;
        }

        #endregion

        private static IEnumerable<string> ProvidePropertiesWithType(IEnumerable<PropertyMapHolder> propertyMapHolders) =>
            propertyMapHolders.Select(pmh => string.Format("{0} {1}", pmh.DestinationPropertySymbol.Type.ToDisplayString(), pmh.DestinationPropertySymbol.Name.ToCamelCase()));

        internal static SourceCode Build(MappingMetaData mmd, Compilation cmp) => new(ProvideSource(mmd, cmp), $"Map{mmd.SourceTypeName}To{mmd.DestinationTypeName}.generated.cs");
    }
}
