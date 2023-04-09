using System.Collections.Generic;

namespace MapThis.Dto
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
