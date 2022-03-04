using MapThis.Services.MethodGenerators;
using MapThis.Services.MethodGenerators.Interfaces;

namespace MapThis.DependencyInjection
{
    public static class DI
    {
        public static IMethodGeneratorService GetMethodGeneratorService()
        {
            return new MethodGeneratorService();
        }
    }
}
