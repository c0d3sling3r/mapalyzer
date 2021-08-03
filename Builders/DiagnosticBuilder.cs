using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Mapalyzer.Builders
{
    internal class DiagnosticBuilder
    {
        #nullable enable
        
        private const string _usageCategory = "Usage";
        private const string _infoReportIdPrefix = "EM1";
        private const string _errorReportIdPrefix = "EM2";
        private const string _warningReportIdPrefix = "EM3";

        readonly string _id;
        readonly string _title;
        readonly string _messageFormat;
        readonly DiagnosticSeverity _severity;

        internal DiagnosticBuilder(string id, string title, string messageFormat, DiagnosticSeverity severity)
        {
            _id = id;
            _title = title;
            _messageFormat = messageFormat;
            _severity = severity;
        }

        internal static DiagnosticBuilder DestinationNotSpecified => new($"{_errorReportIdPrefix}000", 
            "Destination not Specified", 
            "The destination type of mapping is not specified for <{0}>.", 
            DiagnosticSeverity.Error);

        internal static DiagnosticBuilder InvalidSourceNamespace => new($"{_errorReportIdPrefix}001", 
            "Source's Namespace is Invalid", 
            "The containing namespace of type <{0}> should be in '{1}'.",
            DiagnosticSeverity.Error);
        
        internal static DiagnosticBuilder SameNamePropertyDifferentType => new($"{_infoReportIdPrefix}000", 
            "Same Name Property with Different Type", 
            "{0}.{1} with type '{2}' from source cannot be mapped with its match with type '{3}'.",
            DiagnosticSeverity.Info);

        internal static DiagnosticBuilder ParentNodeIsNotDecorated => new($"{_infoReportIdPrefix}001", 
            "Property mapping is unclear.", 
            "You tried to map a property({0}) which its parent class({1}) has not been mapped yet. If this class is itself a parent and you provided mapping for the children, ignore this message. " + 
                "Otherwise there will be no property mapping.",
            DiagnosticSeverity.Info);

        internal static DiagnosticBuilder SourceClassDoesNotHaveDesiredProperty => new($"{_warningReportIdPrefix}000", 
            "Property mapping was unsuccessful.", 
            "There is no matched primitive property in source class({0}) with the name({1}) your provided.",
            DiagnosticSeverity.Warning);

        internal static DiagnosticBuilder SourcePropertyIsNotConvertible => new($"{_warningReportIdPrefix}001", 
            "Property mapping was unsuccessful.", 
            "The desired source property {0} with the type {1} is not convertible.",
            DiagnosticSeverity.Warning);

        internal static DiagnosticBuilder DestinationPropertyIsNotConvertible => new($"{_warningReportIdPrefix}002", 
            "Property mapping was unsuccessful.",
            "The desired destination property {0} with the type {1} is a non-premitive which cannot be assigned implicitly.",
            DiagnosticSeverity.Warning);

        public DiagnosticDescriptor Build() => new(_id, _title, _messageFormat, _usageCategory, _severity, true);
    }
}
