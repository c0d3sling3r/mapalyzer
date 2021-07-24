using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using EasyMapper.Data;
using EasyMapper.Sources;

namespace EasyMapper.Core
{
    [Generator]
    public class MapGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is MapFromSyntaxReciever receiver)
            {
                foreach ((DeclarationType declarationType, INamedTypeSymbol dstSymbol, INamedTypeSymbol srcSymbol) in receiver.MappingAddressList)
                {
                    List<PropertyMapHolder> propertyMapHolders = new();

                    IEnumerable<IPropertySymbol> dstPropertySymbols = dstSymbol.GetAllMembers().OfType<IPropertySymbol>().ToList();
                    IEnumerable<IPropertySymbol> srcPropertySymbols = srcSymbol.GetAllMembers().OfType<IPropertySymbol>().ToList();

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

                    //static bool getConverterByFieldAttribute(AttributeData ad) => ad.AttributeClass.ToDisplayString() == MapConverterForAttributeSource._fullyQualifiedName;
                    //IEnumerable<IFieldSymbol> mapConverters = dstSymbol.GetMembers().OfType<IFieldSymbol>().Where(fs => fs.GetAttributes().Any(ad => getConverterByFieldAttribute(ad)));

                    foreach (IPropertySymbol ps in dstPropertySymbols)
                    {
                        //IFieldSymbol propertyConverterField = mapConverters.SingleOrDefault(fs => (string)(fs.GetAttributes().Single(at => getConverterByFieldAttribute(at))).ConstructorArguments.ElementAt(0).Value == ps.Name);

                        //if (propertyConverterField is not null)
                            //propertyMapHolders.Add(new PropertyMapHolder(ps, ConverterFieldSymbol: propertyConverterField));

                        AttributeData mapPropertyFormAttributeData = ps.GetAttributes().SingleOrDefault(at => at.AttributeClass?.ToDisplayString() == new MapPropertyFromAttributeSource().FullyQualifiedName);

                        if (mapPropertyFormAttributeData is not null)
                        {
                            string differentSourcePropertyName = (string) mapPropertyFormAttributeData.ConstructorArguments.ElementAt(0).Value;
                            bool ignoreTypeDifference = (bool) (mapPropertyFormAttributeData.ConstructorArguments.ElementAt(1).Value ?? false);
                            
                            IPropertySymbol sourcePropertySymbol = srcPropertySymbols.SingleOrDefault(sps => 
                                ps.IsEqual(srcSymbol, sps, context.Compilation, receiver.SyntaxNode, receiver.Diagnostics, differentSourcePropertyName, ignoreTypeDifference));

                            if (sourcePropertySymbol is not null)
                            {
                                ITypeSymbol destinationTypeSymbol = null;

                                if (ignoreTypeDifference)
                                    destinationTypeSymbol = ps.Type;

                                propertyMapHolders.RemoveAll(pmh => pmh.DestinationPropertySymbol.IsEqual(dstSymbol, ps, context.Compilation, receiver.SyntaxNode, receiver.Diagnostics));
                                propertyMapHolders.Add(new PropertyMapHolder(ps, sourcePropertySymbol, DestinationTypeSymbol: destinationTypeSymbol));
                            }
                        }

                        //AttributeData psBaseTypeMapAttribute = ps.Type.GetAttributes().SingleOrDefault(ad => ad.AttributeClass.Name.Equals(MapFromAttributeSource.FullyQualifiedName));

                            //if (psBaseTypeMapAttribute is not null)
                            //{
                            //    IEnumerable<IMethodSymbol> methods = ((INamedTypeSymbol)psBaseTypeMapAttribute.ConstructorArguments.ElementAt(0).Value).GetMembers().OfType<IMethodSymbol>();
                            //    propertyMapHolders.Add(new PropertyMapHolder(ps, DestinationPropertyMappedTypeMethods: methods));
                            //}
                    }

                    MappingMetaData mmd = new(dstSymbol.ContainingNamespace.ToDisplayString(),
                        srcSymbol.Name,
                        dstSymbol.Name,
                        propertyMapHolders,
                        injectableNameSpaces);

                    context.AddSource(MappedClassSource.Build(mmd, context.Compilation));
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
                SourceCode mapConverterAttributeSource = new MapConverterForAttributeSource().Build();
                
                i.AddSource(mapFromAttributeSource.Filename, mapFromAttributeSource.Body);
                i.AddSource(mapPropertyFromAttributeSource.Filename, mapPropertyFromAttributeSource.Body);
                //i.AddSource(mapConverterAttributeSource.Filename, mapConverterAttributeSource.Body);
            });

            context.RegisterForSyntaxNotifications(() => new MapFromSyntaxReciever());
        }
    }
}
