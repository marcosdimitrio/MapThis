using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapEnumInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IList<EnumItemToMapDto> EnumsItemsToMap { get; }
        public IList<IMethodGenerator> ChildrenMethodGenerators { get; }
        public OptionsDto Options { get; }

        public MapEnumInformationDto(MethodInformationDto methodInformation, IList<EnumItemToMapDto> enumItemsToMap, IList<IMethodGenerator> childrenMethodGenerators, OptionsDto options)
        {
            MethodInformation = methodInformation;
            EnumsItemsToMap = enumItemsToMap;
            ChildrenMethodGenerators = childrenMethodGenerators;
            Options = options;
        }
    }
}
