// MIT License

// Copyright (c) 2018 Cezary Piątek

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Helpers taken from https://github.com/cezarypiatek/MappingGenerator
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Helpers
{
    public static class TypeSymbolHelpers
    {
        public static IList<IPropertySymbol> GetPublicProperties(this ITypeSymbol typeSymbol)
        {
            var members = typeSymbol.GetMembers()
                .Where(x => x.Kind == SymbolKind.Property && x.DeclaredAccessibility == Accessibility.Public)
                .Cast<IPropertySymbol>()
                .ToList();

            return members;
        }

        public static bool IsCollection(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.Kind == SymbolKind.ArrayType || typeSymbol.OriginalDefinition.AllInterfaces.Any(x => x.Name == "IEnumerable" && x.ToDisplayString() == "System.Collections.IEnumerable");
        }

        public static bool IsSimpleType(this ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                case SpecialType.System_Decimal:
                case SpecialType.System_DateTime:
                case SpecialType.System_Enum:
                    return true;
            }

            switch (type.TypeKind)
            {
                case TypeKind.Enum:
                    return true;
            }

            if (type.Name == "Guid" && type.ContainingNamespace.Name == "System")
            {
                return true;
            }

            return false;
        }

        public static ITypeSymbol GetElementType(this ITypeSymbol collectionType)
        {
            switch (collectionType)
            {
                case INamedTypeSymbol namedType:
                    if (namedType.IsGenericType == false)
                    {
                        if (namedType.BaseType == null)
                        {
                            var indexer = namedType.GetMembers(WellKnownMemberNames.Indexer).OfType<IPropertySymbol>().FirstOrDefault();
                            if (indexer != null)
                            {
                                return indexer.Type;
                            }

                            throw new NotSupportedException("Cannot determine collection element type");
                        }
                        if (namedType.BaseType.IsSystemObject())
                        {
                            return namedType.BaseType;
                        }
                        return GetElementType(namedType.BaseType);
                    }

                    //INFO: Type is reported as generic when containing type is generic
                    if (
                        namedType.Arity == 0 &&
                        namedType.ContainingType?.TypeKind is object &&
                        (namedType.ContainingType.TypeKind == TypeKind.Class || namedType.ContainingType.TypeKind == TypeKind.Structure)
                    )
                    {
                        var enumerable = namedType.Interfaces.FirstOrDefault(x => x.Name == "IEnumerable" && x.IsGenericType);

                        if (enumerable != null)
                        {
                            return enumerable.TypeArguments[0];
                        }
                    }

                    return namedType.TypeArguments[0];
                case IArrayTypeSymbol arrayType:
                    return arrayType.ElementType;
                default:
                    throw new NotSupportedException("Unknown collection type.");
            }

        }

        public static bool IsSystemObject(this ITypeSymbol current)
        {
            return current.Name == "Object" && current.ContainingNamespace.Name == "System";
        }

    }
}
