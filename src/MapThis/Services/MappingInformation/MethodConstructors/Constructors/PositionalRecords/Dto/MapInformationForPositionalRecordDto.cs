using MapThis.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto
{
    public class MapInformationForPositionalRecordDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IList<PropertyToMapDto> PropertiesToMap { get; }
        public IList<IMethodGenerator> ChildrenMethodGenerators { get; }
        public OptionsDto Options { get; }

        public MapInformationForPositionalRecordDto(MethodInformationDto methodInformation, IList<PropertyToMapDto> propertiesToMap, IList<IMethodGenerator> childrenMethodGenerators, OptionsDto options)
        {
            MethodInformation = methodInformation;
            PropertiesToMap = propertiesToMap;
            ChildrenMethodGenerators = childrenMethodGenerators;
            Options = options;
        }
    }
}
