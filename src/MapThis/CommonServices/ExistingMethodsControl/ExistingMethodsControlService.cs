using MapThis.CommonServices.ExistingMethodsControl.Dto;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.CommonServices.ExistingMethodsControl
{
    public class ExistingMethodsControlService : IExistingMethodsControlService
    {
        private readonly IList<ExistingMethodDto> ExistingMethodList;

        public ExistingMethodsControlService(IList<ExistingMethodDto> existingMethodList)
        {
            ExistingMethodList = existingMethodList;
        }

        public bool TryAddMethod(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
        {
            var childMapCollectionAlreadyExists = ExistingMethodList.Any(x =>
                SymbolEqualityComparer.Default.Equals(x.TargetType, targetType) &&
                SymbolEqualityComparer.Default.Equals(x.SourceType, sourceType)
            );

            if (childMapCollectionAlreadyExists) return false;

            ExistingMethodList.Add(new ExistingMethodDto()
            {
                SourceType = sourceType,
                TargetType = targetType,
            });

            return true;
        }
    }
}
