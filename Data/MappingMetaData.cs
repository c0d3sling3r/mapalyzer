using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace Mapalyzer.Data
{
    internal record MappingMetaData (string DestinationNamespace,
        string SourceTypeName,
        string DestinationTypeName,
        List<PropertyMapHolder> PropertyMapHolders, // Key=PropertySymbol Value=ConverterFieldSymbol
        IEnumerable<InjectableNameSpace> InjectableNameSpaces
    );

    internal record PropertyMapHolder(IPropertySymbol DestinationPropertySymbol, IPropertySymbol SourcePropertySymbol = null, bool ForceConversion = false);

    internal record InjectableNameSpace(string NameSpace, bool ContainsSourceClass = false);
}
