using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EasyMapper.Sources;

namespace EasyMapper.Core
{
    public sealed class MapFromSyntaxReceiver : ISyntaxContextReceiver
    {
        #region Fields

        private readonly MapFromAttributeSource _mapFromAttributeSource = new();

        #endregion

        #region Properties

        public List<(DeclarationType declarationType, INamedTypeSymbol dstSymbol, INamedTypeSymbol srcSymbol)> MappingAddressList { get; private set; } = new();
        public List<Diagnostic> Diagnostics { get; set; } = new();
        public SyntaxNode SyntaxNode { get; private set; }
        public SemanticModel SemanticModel { get; private set; }

        #endregion

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxNode = context.Node;
            SemanticModel = context.SemanticModel;
            DeclarationType declarationType = DeclarationType.Class;
            (AttributeData ad, ISymbol sb) payload = new(null, null);

            if (SyntaxNode is ClassDeclarationSyntax cds)
                cds.HasAttribute(SemanticModel, _mapFromAttributeSource.FullyQualifiedName, out payload);
            else if (SyntaxNode is RecordDeclarationSyntax rds)
            {
                declarationType = DeclarationType.Record;
                rds.HasAttribute(SemanticModel, _mapFromAttributeSource.FullyQualifiedName, out payload);
            }

            if (payload.ad is not null)
            {
                if (!payload.ad.ConstructorArguments.IsEmpty)
                {
                    object argumentValue = payload.ad.ConstructorArguments.ElementAt(0).Value;
                    MappingAddressList.Add((declarationType, (INamedTypeSymbol)payload.sb, (INamedTypeSymbol)argumentValue));
                }
            }
        }
    }

    public enum DeclarationType
    {
        Class,
        Record
    }
}
