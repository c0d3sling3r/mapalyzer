using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Mapalyzer.Builders
{
    internal class DiagnosticBuilder
    {
        #nullable enable
        
        private const string UsageCategory = "Usage";
        private const string InfoReportIdPrefix = "EM1";
        private const string ErrorReportIdPrefix = "EM2";
        private const string WarningReportIdPrefix = "EM3";

        private readonly string _id;
        private readonly string _title;
        private readonly string _messageFormat;
        private readonly DiagnosticSeverity _severity;

        private DiagnosticBuilder(string id, string title, string messageFormat, DiagnosticSeverity severity)
        {
            _id = id;
            _title = title;
            _messageFormat = messageFormat;
            _severity = severity;
        }

        #region Errors
        internal static DiagnosticBuilder DestinationNotSpecified => new($"{ErrorReportIdPrefix}000", 
            "Destination not Specified", 
            "The destination type of mapping is not specified for <{0}>.", 
            DiagnosticSeverity.Error);

        internal static DiagnosticBuilder InvalidSourceNamespace => new($"{ErrorReportIdPrefix}001", 
            "Source's Namespace is Invalid", 
            "The containing namespace of type <{0}> should be in '{1}'.",
            DiagnosticSeverity.Error);

        internal static DiagnosticBuilder MissingPartialModifier => new DiagnosticBuilder($"{ErrorReportIdPrefix}002",
            "The partial modifier is missing.",
            "In order to complete the mapping process, you need to add the \"partial\" modifier to the class declaration.",
            DiagnosticSeverity.Error);
        #endregion

        #region Warnings
        internal static DiagnosticBuilder SourceClassDoesNotHaveDesiredProperty => new($"{WarningReportIdPrefix}000", 
            "Property mapping was unsuccessful.", 
            "There is no matched primitive property in source class({0}) with the name({1}) your provided.",
            DiagnosticSeverity.Warning);

        internal static DiagnosticBuilder SourcePropertyIsNotConvertible => new($"{WarningReportIdPrefix}001", 
            "Property mapping was unsuccessful.", 
            "The desired source property {0} with the type {1} is not convertible.",
            DiagnosticSeverity.Warning);

        internal static DiagnosticBuilder DestinationPropertyIsNotConvertible => new($"{WarningReportIdPrefix}002", 
            "Property mapping was unsuccessful.",
            "The desired destination property {0} with the type {1} is a non-primitive which cannot be assigned implicitly.",
            DiagnosticSeverity.Warning);
        #endregion
        
        #region Infos
        internal static DiagnosticBuilder SameNamePropertyDifferentType => new($"{InfoReportIdPrefix}000", 
            "Same Name Property with Different Type", 
            "{0}.{1} with type '{2}' from source cannot be mapped with its match with type '{3}'.",
            DiagnosticSeverity.Info);

        internal static DiagnosticBuilder PropertyContainerDecorationAnalyzer => new($"{InfoReportIdPrefix}001", 
            "Property mapping is unclear.", 
            "You tried to map a property({0}) which its parent class({1}) has not been mapped yet. If this class is itself a parent and you provided mapping for the children, ignore this message. " + 
            "Otherwise there will be no property mapping.",
            DiagnosticSeverity.Info);
        #endregion

        public DiagnosticDescriptor Build() => new(_id, _title, _messageFormat, UsageCategory, _severity, true);
    }
}
