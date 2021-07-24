using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace EasyMapper.Data
{
    internal record MappingMetaData (string Namespace,
        string SourceTypeName,
        string DestinationTypeName,
        IEnumerable<PropertyMapHolder> PropertyMapHolders, // Key=PropertySymbol Value=ConverterFieldSymbol
        IEnumerable<InjectableNameSpace> InjectableNameSpaces
    );

    internal record PropertyMapHolder(IPropertySymbol DestinationPropertySymbol, 
        IPropertySymbol SourcePropertySymbol = null, 
        IFieldSymbol ConverterFieldSymbol = null,
        IEnumerable<IMethodSymbol> DestinationPropertyMappedTypeMethods = null,
        ITypeSymbol DestinationTypeSymbol = null);

    internal record InjectableNameSpace(string NameSpace, bool ContainsSourceClass = false);
}
