using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerators.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerators.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.PositionalRecordMethodGenerators.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerators.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Factories
{
    [Export(typeof(IMethodGeneratorFactory))]
    public class MethodGeneratorFactory : IMethodGeneratorFactory
    {
        private readonly ISingleMethodGenerator SingleMethodGenerator;
        private readonly ICollectionMethodGenerator CollectionMethodGenerator;
        private readonly IEnumMethodGenerator EnumMethodGenerator;
        private readonly IPositionalRecordMethodGenerator PositionalRecordMethodGenerator;

        [ImportingConstructor]
        public MethodGeneratorFactory(ISingleMethodGenerator singleMethodGeneratorService, ICollectionMethodGenerator collectionMethodGenerator, IEnumMethodGenerator enumMethodGenerator, IPositionalRecordMethodGenerator positionalRecordMethodGenerator)
        {
            SingleMethodGenerator = singleMethodGeneratorService;
            CollectionMethodGenerator = collectionMethodGenerator;
            EnumMethodGenerator = enumMethodGenerator;
            PositionalRecordMethodGenerator = positionalRecordMethodGenerator;
        }

        public IMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ClassMapGenerator(dto, SingleMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public IMethodGenerator Get(MapInformationForCollectionDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new ListMapGenerator(dto, CollectionMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public IMethodGenerator Get(MapEnumInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new EnumMapGenerator(dto, EnumMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }

        public IMethodGenerator Get(MapInformationForPositionalRecordDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            return new PositionalRecordMapGenerator(dto, PositionalRecordMethodGenerator, codeAnalisysDependenciesDto, existingNamespaces);
        }

    }
}
