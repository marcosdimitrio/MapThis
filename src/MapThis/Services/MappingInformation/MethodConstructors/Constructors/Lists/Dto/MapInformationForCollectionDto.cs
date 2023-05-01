using MapThis.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto
{
    public class MapInformationForCollectionDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IMethodGenerator ChildMethodGenerator { get; }
        public OptionsDto Options { get; }

        public MapInformationForCollectionDto(MethodInformationDto methodInformation, IMethodGenerator childMethodGenerator, OptionsDto options)
        {
            MethodInformation = methodInformation;
            ChildMethodGenerator = childMethodGenerator;
            Options = options;
        }

    }
}
