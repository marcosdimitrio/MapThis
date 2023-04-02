using MapThis.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapEnumInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IList<EnumItemToMapDto> EnumsItemsToMap { get; }
        public IList<ICompoundMethodGenerator> ChildrenMethodGenerators { get; }
        public OptionsDto Options { get; }

        public MapEnumInformationDto(MethodInformationDto methodInformation, IList<EnumItemToMapDto> enumItemsToMap, IList<ICompoundMethodGenerator> childrenMethodGenerators, OptionsDto options)
        {
            MethodInformation = methodInformation;
            EnumsItemsToMap = enumItemsToMap;
            ChildrenMethodGenerators = childrenMethodGenerators;
            Options = options;
        }
    }
}
