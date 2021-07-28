using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mapalyzer
{
    public static class MemberDeclarationSyntaxExtensions
    {
        public static bool HasAttribute(this MemberDeclarationSyntax typeDeclarationSyntax, SemanticModel semanticModel, string attributeQualifiedName, out (AttributeData ad, ISymbol sb) payload)
        {
            payload.ad = null;

            payload.sb = semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
            System.Collections.Immutable.ImmutableArray<AttributeData> attributes = payload.sb.GetAttributes();

            if (attributes.Any())
                payload.ad = attributes.SingleOrDefault(ad => ad.AttributeClass.ToDisplayString() == attributeQualifiedName);

            return payload.ad is not null;
        }
    }
}