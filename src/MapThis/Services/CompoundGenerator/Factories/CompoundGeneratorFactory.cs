using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Factories.Interfaces;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MethodGenerator.Interfaces;
using System.Composition;

namespace MapThis.Services.CompoundGenerator.Factories
{
    [Export(typeof(ICompoundGeneratorFactory))]
    public class CompoundGeneratorFactory : ICompoundGeneratorFactory
    {
        private readonly IMethodGeneratorService MethodGeneratorService;

        [ImportingConstructor]
        public CompoundGeneratorFactory(IMethodGeneratorService methodGeneratorService)
        {
            MethodGeneratorService = methodGeneratorService;
        }

        public ICompoundGenerator Get(MapInformationDto dto)
        {
            return new ClassMapGenerator(dto, MethodGeneratorService);
        }

        public ICompoundGenerator Get(MapCollectionInformationDto dto)
        {
            return new ListMapGenerator(dto, MethodGeneratorService);
        }

    }
}
