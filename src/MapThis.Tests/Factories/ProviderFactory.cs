using MapThis.CommonServices.AccessModifierIdentifiers;
using MapThis.CommonServices.ExistingMethodsControl.Factories;
using MapThis.CommonServices.IdentifierNames;
using MapThis.CommonServices.UniqueVariableNames;
using MapThis.CommonServices.UserOptions.Interfaces;
using MapThis.Refactorings.MappingRefactors;
using MapThis.Services.MappingInformation;
using MapThis.Services.MappingInformation.MethodConstructors;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerators;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerators;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.PositionalRecordMethodGenerators;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerators;
using MapThis.Vsix.Options;
using Microsoft.CodeAnalysis.CodeRefactorings;
using NSubstitute;

namespace MapThis.Tests.Factories
{
    public static class ProviderFactory
    {
        public static CodeRefactoringProvider GetCodeRefactoringProvider()
        {
            var generalOptions = new GeneralOptions()
            {
                UsePatternMatchingForNullChecking = true,
            };
            return GetCodeRefactoringProvider(generalOptions);
        }

        public static CodeRefactoringProvider GetCodeRefactoringProvider(GeneralOptions generalOptions)
        {
            var userOptionsService = Substitute.For<IUserOptionsService>();
            userOptionsService.GeneralOptions.Returns(generalOptions);

            var identifierNameService = new IdentifierNameService();
            var uniqueVariableNameGenerator = new UniqueVariableNameGenerator();
            var singleMethodGeneratorService = new SingleMethodGenerator(identifierNameService, uniqueVariableNameGenerator, userOptionsService);
            var collectionMethodGeneratorService = new CollectionMethodGenerator(identifierNameService, uniqueVariableNameGenerator, userOptionsService);
            var enumMethodGenerator = new EnumMethodGenerator();
            var positionalRecordMethodGenerator = new PositionalRecordMethodGenerator(identifierNameService, uniqueVariableNameGenerator, userOptionsService);
            var methodGeneratorFactory = new MethodGeneratorFactory(singleMethodGeneratorService, collectionMethodGeneratorService, enumMethodGenerator, positionalRecordMethodGenerator);
            var existingMethodsControlServiceFactory = new ExistingMethodsControlServiceFactory();
            var accessModifierIdentifier = new AccessModifierIdentifier();
            var recursiveMethodConstructor = new RecursiveMethodConstructor(methodGeneratorFactory, accessModifierIdentifier);
            var mappingInformationService = new MappingInformationService(existingMethodsControlServiceFactory, recursiveMethodConstructor);
            var mappingGeneratorService = new MappingRefactorService(mappingInformationService);

            return new MapThisCodeRefactoringProvider(mappingGeneratorService, recursiveMethodConstructor);
        }
    }
}
