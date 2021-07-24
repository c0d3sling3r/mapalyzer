using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EasyMapper.Sources;

namespace EasyMapper.Core
{
    public sealed class MapFromSyntaxReciever : ISyntaxContextReceiver
    {
        public List<(DeclarationType declarationType, INamedTypeSymbol dstSymbol, INamedTypeSymbol srcSymbol)> MappingAddressList { get; private set; } = new();
        public List<Diagnostic> Diagnostics { get; set; } = new();
        public SyntaxNode SyntaxNode { get; private set; }
        public SemanticModel SemanticModel { get; private set; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxNode = context.Node;
            SemanticModel = context.SemanticModel;
            (AttributeData ad, INamedTypeSymbol ts) payload = new(null, null);
            DeclarationType declarationType = DeclarationType.Class;

            if (SyntaxNode is ClassDeclarationSyntax cds)
                payload = ValidateAttributes(SemanticModel, cds);
            else if (SyntaxNode is RecordDeclarationSyntax rds)
            {
                declarationType = DeclarationType.Record;
                payload = ValidateAttributes(SemanticModel, rds);
            }

            if (payload.ad is not null)
            {
                if (!payload.ad.ConstructorArguments.IsEmpty)
                {
                    object argumentValue = payload.ad.ConstructorArguments.ElementAt(0).Value;
                    MappingAddressList.Add((declarationType, payload.ts, (INamedTypeSymbol)argumentValue));
                }
            }
        }

        private (AttributeData, INamedTypeSymbol) ValidateAttributes(SemanticModel semanticModel, TypeDeclarationSyntax cds)
        {
            AttributeData attributeData = null;

            INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(cds) as INamedTypeSymbol;
            System.Collections.Immutable.ImmutableArray<AttributeData> attributes = typeSymbol.GetAttributes();

            if (attributes.Any())
                attributeData = attributes.SingleOrDefault(ad => ad.AttributeClass.ToDisplayString() == new MapFromAttributeSource().FullyQualifiedName);

            return (attributeData, typeSymbol);
        }
    }

    public enum DeclarationType
    {
        Class,
        Record
    }
}
