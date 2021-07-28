using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using Mapalyzer.Builders;

namespace Mapalyzer
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
                    diagnostics.Add(Diagnostic.Create(DiagnosticBuilder.SameNamePropertyDifferentType.Build(), dstPropertySymbol.Locations.First(), srcTypeSymbol, srcPropertySymbol, dstPropertySymbol));
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

        internal static bool IsPrimitive(this ITypeSymbol typeSymbol) 
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Char or 
                SpecialType.System_Boolean or 
                SpecialType.System_Byte or 
                SpecialType.System_SByte or 
                SpecialType.System_Int16 or 
                SpecialType.System_UInt16 or 
                SpecialType.System_Int32 or 
                SpecialType.System_UInt32 or 
                SpecialType.System_Int64 or 
                SpecialType.System_UInt64 or 
                SpecialType.System_Decimal or 
                SpecialType.System_Single or 
                SpecialType.System_Double or 
                SpecialType.System_String or 
                SpecialType.System_DateTime => true,
                _ => false,
            };
        }
    }
}
