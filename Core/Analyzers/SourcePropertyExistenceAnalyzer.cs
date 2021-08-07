using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Mapalyzer.Builders;
using Mapalyzer.Sources;

namespace Mapalyzer.Core.Analyzers
{
    public class SourcePropertyExistenceAnalyzer : IAnalyzer
    {
        private static readonly MapFromAttributeSource MapFromAttributeSource = new();
        private static readonly MapPropertyFromAttributeSource MapPropertyFromAttributeSource = new();

        public SourcePropertyExistenceAnalyzer(ICollection<DiagnosticDescriptor> supportedDiagnosticsList)
        {   
            supportedDiagnosticsList.Add(DiagnosticBuilder.SourceClassDoesNotHaveDesiredProperty.Build());
        }

        public void RegisterAction(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PropertyDeclarationSyntax pds)
            {
                if (pds.HasAttribute(context.SemanticModel, MapPropertyFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ps) payload)) 
                {
                    if (((ClassDeclarationSyntax)pds.Parent).HasAttribute(context.SemanticModel, MapFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ts) parentPayload)) 
                    {
                        if (!parentPayload.ad.ConstructorArguments.IsEmpty)
                        {
                            object parentSourceTypeValue = parentPayload.ad.ConstructorArguments.ElementAt(0).Value;
                            object sourcePropertyNameValue = payload.ad.ConstructorArguments.ElementAt(0).Value;

                            if (((INamedTypeSymbol)parentSourceTypeValue).GetAllMembers()
                                .OfType<IPropertySymbol>()
                                .Where(ds => ds.Type.IsPrimitive())
                                .All(ps => ps.Name != (string)sourcePropertyNameValue))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(DiagnosticBuilder.SourceClassDoesNotHaveDesiredProperty.Build(),
                                    pds.GetLocation(),
                                    ((INamedTypeSymbol)parentSourceTypeValue)?.Name,
                                    (string)sourcePropertyNameValue));
                            }
                        }
                    }
                }
            }
        }
    }
}