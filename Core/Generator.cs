using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using EasyMapper.Data;
using EasyMapper.Sources;
using EasyMapper.Builders;

namespace EasyMapper.Core
{
    [Generator]
    public class MapGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is MapFromSyntaxReceiver receiver)
            {
                foreach ((DeclarationType declarationType, INamedTypeSymbol dstSymbol, INamedTypeSymbol srcSymbol) in receiver.MappingAddressList)
                {
                    List<PropertyMapHolder> propertyMapHolders = new();

                    IEnumerable<IPropertySymbol> dstPropertySymbols = dstSymbol.GetAllMembers().OfType<IPropertySymbol>().Where(ds => ds.Type.IsPrimitive()).ToList();
                    IEnumerable<IPropertySymbol> srcPropertySymbols = srcSymbol.GetAllMembers().OfType<IPropertySymbol>().Where(ss => ss.Type.IsPrimitive()).ToList();

                    IPropertySymbol dps;
                    List<InjectableNameSpace> injectableNameSpaces = new();

                    if (!srcSymbol.ContainingNamespace.IsGlobalNamespace)
                        injectableNameSpaces.Add(new(srcSymbol.ContainingNamespace.ToDisplayString(), true));

                    //TODO: check if has baseType
                    foreach (IPropertySymbol ps in srcPropertySymbols)
                    {
                        dps = dstPropertySymbols.SingleOrDefault(s => s.IsEqual(srcSymbol, ps, context.Compilation, receiver.SyntaxNode, receiver.Diagnostics));

                        if (dps is not null)
                            propertyMapHolders.Add(new PropertyMapHolder(dps, SourcePropertySymbol: ps));
                    }

                    foreach (IPropertySymbol ps in dstPropertySymbols)
                    {
                        EnlistDecoratedDestinationProperties(context, receiver, dstSymbol, ps, srcSymbol, srcPropertySymbols, propertyMapHolders);
                    }

                    MappingMetaData mmd = new(dstSymbol.ContainingNamespace.ToDisplayString(),
                        srcSymbol.Name,
                        dstSymbol.Name,
                        propertyMapHolders,
                        injectableNameSpaces);

                    context.AddSource(MappedClassSource.Build(mmd, context.Compilation, receiver.Diagnostics));
                }

                if (receiver.Diagnostics.Any())
                    receiver.Diagnostics.ForEach(context.ReportDiagnostic);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization((i) =>
            {
                SourceCode mapFromAttributeSource = new MapFromAttributeSource().Build();
                SourceCode mapPropertyFromAttributeSource = new MapPropertyFromAttributeSource().Build();
                
                i.AddSource(mapFromAttributeSource.Filename, mapFromAttributeSource.Body);
                i.AddSource(mapPropertyFromAttributeSource.Filename, mapPropertyFromAttributeSource.Body);
            });

            context.RegisterForSyntaxNotifications(() => new MapFromSyntaxReceiver());
        }

        #region Utils

        void EnlistDecoratedDestinationProperties(GeneratorExecutionContext context, MapFromSyntaxReceiver receiver, INamedTypeSymbol dstSymbol, IPropertySymbol destPropertySymbol, INamedTypeSymbol srcSymbol, IEnumerable<IPropertySymbol> srcPropertySymbols, List<PropertyMapHolder> propertyMapHolders)
            {
                AttributeData mapPropertyFormAttributeData = destPropertySymbol.GetAttributes().SingleOrDefault(at => at.AttributeClass?.ToDisplayString() == new MapPropertyFromAttributeSource().FullyQualifiedName);

                if (mapPropertyFormAttributeData is not null)
                {
                    string differentSourcePropertyName = (string) mapPropertyFormAttributeData.ConstructorArguments.ElementAt(0).Value;
                    bool ignoreTypeDifference = (bool) (mapPropertyFormAttributeData.ConstructorArguments.ElementAt(1).Value ?? false);
                    
                    IPropertySymbol sourcePropertySymbol = srcPropertySymbols.SingleOrDefault(sps => 
                        destPropertySymbol.IsEqual(srcSymbol, sps, context.Compilation, receiver.SyntaxNode, receiver.Diagnostics, differentSourcePropertyName, ignoreTypeDifference));

                    if (sourcePropertySymbol is not null)
                    {
                        if (ignoreTypeDifference)
                        {
                            bool isConvertible = true;

                            if (!destPropertySymbol.Type.IsPrimitive()) 
                            {
                                receiver.Diagnostics.Add(Diagnostic.Create(DiagnosticBuilder.DestinationPropertyIsNotConvertible.Build(), destPropertySymbol.Locations.First(), destPropertySymbol, destPropertySymbol.Type));
                                isConvertible = false;
                            }

                            if (!sourcePropertySymbol.Type.IsPrimitive()) 
                            {
                                receiver.Diagnostics.Add(Diagnostic.Create(DiagnosticBuilder.SourcePropertyIsNotConvertible.Build(), sourcePropertySymbol.Locations.First(), sourcePropertySymbol, sourcePropertySymbol.Type));
                                isConvertible = false;
                            }

                            ignoreTypeDifference = isConvertible;
                        }

                        propertyMapHolders.RemoveAll(pmh => pmh.DestinationPropertySymbol.IsEqual(dstSymbol, destPropertySymbol, context.Compilation, receiver.SyntaxNode, receiver.Diagnostics));
                        propertyMapHolders.Add(new PropertyMapHolder(destPropertySymbol, sourcePropertySymbol, ignoreTypeDifference));
                    }
                }
            }

        #endregion
    }
}
