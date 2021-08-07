using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Mapalyzer.Builders;
using Mapalyzer.Sources;

namespace Mapalyzer.Core.Analyzers
{
    public class PropertyContainerDecorationAnalyzer : IAnalyzer
    {
        private static readonly MapFromAttributeSource MapFromAttributeSource = new();
        private static readonly MapPropertyFromAttributeSource MapPropertyFromAttributeSource = new();

        public PropertyContainerDecorationAnalyzer(ICollection<DiagnosticDescriptor> supportedDiagnosticsList)
        {
            DiagnosticBuilder.PropertyContainerDecorationAnalyzer.Build();
        }

        public void RegisterAction(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PropertyDeclarationSyntax pds)
            {
                if (pds.HasAttribute(context.SemanticModel, MapPropertyFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ps) payload)) 
                {
                    if (!((ClassDeclarationSyntax)pds.Parent).HasAttribute(context.SemanticModel, MapFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ts) parentPayload)) 
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticBuilder.PropertyContainerDecorationAnalyzer.Build(), pds.GetLocation(), payload.ps.Name, parentPayload.ts.Name));
                    }
                }
            }
        }
    }
}