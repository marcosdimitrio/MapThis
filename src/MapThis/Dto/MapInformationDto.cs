using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IList<PropertyToMapDto> PropertiesToMap { get; }
        public IList<ICompoundMethodGenerator> ChildrenMethodGenerators { get; }
        public OptionsDto Options { get; }

        public MapInformationDto(MethodInformationDto methodInformation, IList<PropertyToMapDto> propertiesToMap, IList<ICompoundMethodGenerator> childrenMethodGenerators, OptionsDto options)
        {
            MethodInformation = methodInformation;
            PropertiesToMap = propertiesToMap;
            ChildrenMethodGenerators = childrenMethodGenerators;
            Options = options;
        }
    }
}
