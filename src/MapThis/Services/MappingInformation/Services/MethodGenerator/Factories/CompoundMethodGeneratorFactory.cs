using MapThis.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerator.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Factories
{
    [Export(typeof(ICompoundMethodGeneratorFactory))]
    public class CompoundMethodGeneratorFactory : ICompoundMethodGeneratorFactory
    {
        private readonly ISingleMethodGeneratorService SingleMethodGeneratorService;
        private readonly IEnumMethodGenerator EnumMethodGenerator;

        [ImportingConstructor]
        public CompoundMethodGeneratorFactory(ISingleMethodGeneratorService singleMethodGeneratorService, IEnumMethodGenerator enumMethodGenerator)
        {
            SingleMethodGeneratorService = singleMethodGeneratorService;
            EnumMethodGenerator = enumMethodGenerator;
        }

        public ICompoundMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ClassMapGenerator(dto, SingleMethodGeneratorService, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public ICompoundMethodGenerator Get(MapCollectionInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ListMapGenerator(dto, SingleMethodGeneratorService, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public ICompoundMethodGenerator Get(MapEnumInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new EnumMapGenerator(dto, EnumMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }
    }
}
