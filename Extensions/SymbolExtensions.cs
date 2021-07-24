using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using EasyMapper.Builders;

namespace EasyMapper
{
    internal static class SymbolExtensions
    {
        internal static bool IsEqual(this IPropertySymbol dstPropertySymbol, 
            INamedTypeSymbol srcTypeSymbol,
            IPropertySymbol srcPropertySymbol, 
            Compilation compilation, 
            SyntaxNode syntaxNode, 
            List<Diagnostic> diagnostics, 
            string differentSourcePropertyName = null,
            bool ignoreTypeDifference = false)
        {
            bool check = string.IsNullOrEmpty(differentSourcePropertyName) ? dstPropertySymbol.Name == srcPropertySymbol.Name : srcPropertySymbol.Name == differentSourcePropertyName;

            if (check)
            {
                if (!ignoreTypeDifference)
                    check &= SymbolEqualityComparer.Default.Equals(dstPropertySymbol.Type, srcPropertySymbol.Type) ||
                             compilation.HasImplicitConversion(dstPropertySymbol.Type, srcPropertySymbol.Type) ||
                             dstPropertySymbol.Type.Name == srcPropertySymbol.Type.Name;
                else
                    diagnostics.Add(DiagnosticBuilder.SameNamePropertyDifferentType(dstPropertySymbol.Locations.First(), srcTypeSymbol, srcPropertySymbol, dstPropertySymbol));
            }

            return check;
        }

        internal static INamedTypeSymbol FindBaseType(this INamedTypeSymbol symbol)
        {
            INamedTypeSymbol baseSymbol = symbol;

            if (symbol.BaseType is not null && symbol.BaseType.Name == symbol.Name)
            {
                baseSymbol = symbol.BaseType;
                FindBaseType(baseSymbol);
            }

            return baseSymbol;
        }

        /// <summary>
        /// Gathers all members of an <see cref="ITypeSymbol"/> including its predecessors' members.
        /// </summary>
        /// <param name="symbol">The <see cref="ITypeSymbol"/> for which its members and all its predecessors' members are needed.</param>
        /// <returns>An <see cref="ImmutableArray{ISymbol}"/> holds all members.</returns>
        internal static ImmutableArray<ISymbol> GetAllMembers(this ITypeSymbol symbol)
        {
            ITypeSymbol pivot = symbol;
            ImmutableArray<ISymbol> members = pivot.GetMembers().Select(s => s).ToImmutableArray();

            while (pivot.BaseType != null)
            {
                pivot = pivot.BaseType;
                members = members.Concat(pivot.GetMembers().Select(s => s)).ToImmutableArray();
            }

            return members;
        }
    }
}
