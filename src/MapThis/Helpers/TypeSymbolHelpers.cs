using Microsoft.CodeAnalysis;

namespace MapThis.Helpers
{
    public static class TypeSymbolHelpers
    {
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
    }
}
