using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Mapalyzer.Builders;
using Mapalyzer.Core.Analyzers;

namespace Mapalyzer.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer : DiagnosticAnalyzer
    {
        #region Fields

        private static readonly ICollection<DiagnosticDescriptor> SupportedDiagnosticsList = new List<DiagnosticDescriptor>();

        private static readonly IAnalyzer PartialModifierAnalyzer = new PartialModifierAnalyzer(SupportedDiagnosticsList); 
        private static readonly IAnalyzer SourcePropertyExistenceAnalyzer = new SourcePropertyExistenceAnalyzer(SupportedDiagnosticsList);
        private static readonly IAnalyzer PropertyContainerDecorationAnalyzer = new PropertyContainerDecorationAnalyzer(SupportedDiagnosticsList);

        #endregion

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => SupportedDiagnosticsList.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(PartialModifierAnalyzer.RegisterAction, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(SourcePropertyExistenceAnalyzer.RegisterAction, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(PropertyContainerDecorationAnalyzer.RegisterAction, SyntaxKind.PropertyDeclaration);
        }
    }
}