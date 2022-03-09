using MapThis.Refactorings.MappingGenerator;
using MapThis.Services.CompoundGenerator.Factories;
using MapThis.Services.MappingInformation;
using MapThis.Services.SingleMethodGenerator;
using MapThis.Tests.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using RoslynTestKit;
using System.Collections.Generic;
using Xunit;

namespace MapThis.Tests
{

    public class TestSuite : CodeRefactoringTestFixture
    {
        public static IEnumerable<object[]> GetClients()
        {
            yield return new object[] { "Should map a simple class with one parameter", GetData(Resources._01_Before, Resources._01_Refactored) };
            yield return new object[] { "Should not exclude additional parameters from first method", GetData(Resources._02_Before, Resources._02_Refactored) };
        }

        [Theory]
        [MemberData(nameof(GetClients))]
        public void Test(string name, MemberDataSerializer<TestDataDto> dto)
        {
            TestCodeRefactoring(dto.Object.Before, dto.Object.Refactored);
        }

        protected override string LanguageName => LanguageNames.CSharp;

        protected override CodeRefactoringProvider CreateProvider()
        {
            var singleMethodGeneratorService = new SingleMethodGeneratorService();
            var compoundMethodGeneratorFactory = new CompoundMethodGeneratorFactory(singleMethodGeneratorService);
            var mappingInformationService = new MappingInformationService(compoundMethodGeneratorFactory);
            var mappingGeneratorService = new MappingGeneratorService(mappingInformationService);

            return new MapThisCodeRefactoringProvider(mappingGeneratorService);
        }

        private static MemberDataSerializer<TestDataDto> GetData(string before, string refactored)
        {
            return new MemberDataSerializer<TestDataDto>(new TestDataDto() { Before = before, Refactored = refactored });
        }

    }
}
