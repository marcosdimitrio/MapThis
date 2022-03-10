using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using System.Composition;

namespace MapThis.Services.CompoundGenerator.Factories
{
    [Export(typeof(ICompoundMethodGeneratorFactory))]
    public class CompoundMethodGeneratorFactory : ICompoundMethodGeneratorFactory
    {
        private readonly ISingleMethodGeneratorService SingleMethodGeneratorService;

        [ImportingConstructor]
        public CompoundMethodGeneratorFactory(ISingleMethodGeneratorService singleMethodGeneratorService)
        {
            SingleMethodGeneratorService = singleMethodGeneratorService;
        }

        public ICompoundMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            return new ClassMapGenerator(dto, SingleMethodGeneratorService, codeAnalisysDependenciesDto);
        }

        public ICompoundMethodGenerator Get(MapCollectionInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            return new ListMapGenerator(dto, SingleMethodGeneratorService, codeAnalisysDependenciesDto);
        }

    }
}
