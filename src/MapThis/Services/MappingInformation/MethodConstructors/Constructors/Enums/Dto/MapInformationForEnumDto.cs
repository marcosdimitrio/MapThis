using MapThis.Dto;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto
{
    public class MapEnumInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IList<EnumItemToMapDto> EnumsItemsToMap { get; }
        public OptionsDto Options { get; }

        public MapEnumInformationDto(MethodInformationDto methodInformation, IList<EnumItemToMapDto> enumItemsToMap, OptionsDto options)
        {
            MethodInformation = methodInformation;
            EnumsItemsToMap = enumItemsToMap;
            Options = options;
        }
    }
}
