using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace EasyMapper.Builders
{
    internal static class DiagnosticBuilder
    {
        #nullable enable
        
        private const string _usageCategory = "Usage";
        private const string _infoReportCodePrefix = "SSG0";
        private const string _errorReportCodePrefix = "SSG1";
        private const string _warningReportCodePrefix = "SSG2";

        internal static Diagnostic MapFromAttributeDestinationNotSpecified(Location location, INamedTypeSymbol classSymbol) =>
            Create($"{_errorReportCodePrefix}001", location, "Destination not Specified", $"The destination type of mapping is not specified for <{classSymbol.ToDisplayString()}>.");

        internal static Diagnostic MapFromAttributeValueHasInvalidNamespace(string namesapce, Location location, INamedTypeSymbol classSymbol) =>
            Create($"{_errorReportCodePrefix}002", location, "Source's Namespace is Invalid", $"The containing namespace of type <{classSymbol.ToDisplayString()}> should be in '{namesapce}'.");
        
        internal static Diagnostic SameNamePropertyDifferentType(Location location, INamedTypeSymbol srcTypeSymbol, IPropertySymbol srcPropertySymbol, IPropertySymbol destPropertySymbol) =>
            Create($"{_infoReportCodePrefix}003", location, "Same Name Property with Different Type",
                $"{srcTypeSymbol}.{srcPropertySymbol.Name} with type '{srcPropertySymbol.Type.Name}' from source cannot be mapped with its match with type '{destPropertySymbol.Type.Name}'.", DiagnosticSeverity.Info);

        private static Diagnostic Create(string id, Location? location, string title, string message, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        {
            DiagnosticDescriptor dd = new(id, title, message, _usageCategory, severity, true);
            return Diagnostic.Create(dd, location);
        }
    }
}
