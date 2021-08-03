using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Mapalyzer.Sources;
using Mapalyzer.Builders;

namespace Mapalyzer.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MapPropertyFromAnalyzer : DiagnosticAnalyzer
    {
        #region Fields

        private static readonly MapFromAttributeSource _mapFromAttributeSource = new();
        private static readonly MapPropertyFromAttributeSource _mapPropertyFromAttributeSource = new();

        #endregion

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticBuilder.ParentNodeIsNotDecorated.Build(), DiagnosticBuilder.SourceClassDoesNotHaveDesiredProperty.Build());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(Analyzer, SyntaxKind.PropertyDeclaration);
        }

        private static void Analyzer(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PropertyDeclarationSyntax pds)
            {
                if (pds.HasAttribute(context.SemanticModel, _mapPropertyFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ps) payload)) 
                {
                    if (((ClassDeclarationSyntax)pds.Parent).HasAttribute(context.SemanticModel, _mapFromAttributeSource.FullyQualifiedName, out (AttributeData ad, ISymbol ts) parentPayload)) 
                    {
                        if (!parentPayload.ad.ConstructorArguments.IsEmpty)
                        {
                            object parentSourceTypeValue = parentPayload.ad.ConstructorArguments.ElementAt(0).Value;
                            object sourcePropertyNameValue = payload.ad.ConstructorArguments.ElementAt(0).Value;
                            
                            if (!((INamedTypeSymbol)parentSourceTypeValue).GetAllMembers().OfType<IPropertySymbol>().Where(ds => ds.Type.IsPrimitive()).Any(ps => ps.Name == (string)sourcePropertyNameValue))
                                context.ReportDiagnostic(Diagnostic.Create(DiagnosticBuilder.SourceClassDoesNotHaveDesiredProperty.Build(), pds.GetLocation(), ((INamedTypeSymbol)parentSourceTypeValue).Name, (string)sourcePropertyNameValue));
                        }
                    }
                    else 
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticBuilder.ParentNodeIsNotDecorated.Build(), pds.GetLocation(), payload.ps.Name, parentPayload.ts.Name));
                    }
                }

            }
        }
    }
}