using MapThis.Dto;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Interfaces
{
    public interface IMappingInformationService
    {
        MapInformationDto GetMap(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IPropertySymbol> sourceMembers, IList<IPropertySymbol> targetMembers, IList<IMethodSymbol> existingMethods);
    }
}
