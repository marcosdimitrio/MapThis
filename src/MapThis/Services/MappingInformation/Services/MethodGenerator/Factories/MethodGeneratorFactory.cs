using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerator.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Factories
{
    [Export(typeof(IMethodGeneratorFactory))]
    public class MethodGeneratorFactory : IMethodGeneratorFactory
    {
        private readonly ISingleMethodGeneratorService SingleMethodGeneratorService;
        private readonly ICollectionMethodGeneratorService CollectionMethodGeneratorService;
        private readonly IEnumMethodGenerator EnumMethodGenerator;

        [ImportingConstructor]
        public MethodGeneratorFactory(ISingleMethodGeneratorService singleMethodGeneratorService, ICollectionMethodGeneratorService collectionMethodGeneratorService, IEnumMethodGenerator enumMethodGenerator)
        {
            SingleMethodGeneratorService = singleMethodGeneratorService;
            CollectionMethodGeneratorService = collectionMethodGeneratorService;
            EnumMethodGenerator = enumMethodGenerator;
        }

        public IMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ClassMapGenerator(dto, SingleMethodGeneratorService, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public IMethodGenerator Get(MapCollectionInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ListMapGenerator(dto, CollectionMethodGeneratorService, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public IMethodGenerator Get(MapEnumInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new EnumMapGenerator(dto, EnumMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }
    }
}
