using System.Collections.Generic;
using System.Linq;

using Mapalyzer.Builders;
using Mapalyzer.Sources;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Mapalyzer.Core.Analyzers
{
    public class PartialModifierAnalyzer : IAnalyzer
    {
        private static readonly MapFromAttributeSource MapFromAttributeSource = new();

        public PartialModifierAnalyzer(ICollection<DiagnosticDescriptor> supportedDiagnostics)
        {
            supportedDiagnostics.Add(DiagnosticBuilder.MissingPartialModifier.Build());
        }

        public void RegisterAction(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ClassDeclarationSyntax cds)
            {
                if (cds.HasAttribute(context.SemanticModel, MapFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ts) payload))
                {
                    if (cds.Modifiers.All(sk => sk.ValueText != "partial"))
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticBuilder.MissingPartialModifier.Build(), cds.Identifier.GetLocation()));
                }
            }
        }
    }
}