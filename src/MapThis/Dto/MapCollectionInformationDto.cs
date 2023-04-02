using MapThis.Services.MethodGenerator.Interfaces;

namespace MapThis.Dto
{
    public class MapCollectionInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public ICompoundMethodGenerator ChildMethodGenerator { get; }
        public OptionsDto Options { get; }

        public MapCollectionInformationDto(MethodInformationDto methodInformation, ICompoundMethodGenerator childMethodGenerator, OptionsDto options)
        {
            MethodInformation = methodInformation;
            ChildMethodGenerator = childMethodGenerator;
            Options = options;
        }

    }
}
