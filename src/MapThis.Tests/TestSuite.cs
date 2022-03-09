using MapThis.Refactorings.MappingGenerator;
using MapThis.Services.CompoundGenerator.Factories;
using MapThis.Services.ExistingMethodsControl.Factories;
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
            yield return new object[] { "01 Should map a simple class with one parameter", GetData(Resources._01_Before, Resources._01_Refactored) };
            yield return new object[] { "02 Should not exclude additional parameters from first method", GetData(Resources._02_Before, Resources._02_Refactored) };
            yield return new object[] { "03 Should keep accessibility of first method", GetData(Resources._03_Before, Resources._03_Refactored) };
            yield return new object[] { "04 Should automatically add usings for collections", GetData(Resources._04_Before, Resources._04_Refactored) };
        }

        [Theory]
        [MemberData(nameof(GetClients))]
        #region SupressMessage
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026", Justification = "The name is displayed in test explorer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0060", Justification = "The name is displayed in test explorer")]
        #endregion
        public void Test(string name, MemberDataSerializer<TestDataDto> dto)
        {
            TestCodeRefactoring(dto.Object.Before, dto.Object.Refactored);
        }

        protected override string LanguageName => LanguageNames.CSharp;

        protected override CodeRefactoringProvider CreateProvider()
        {
            var singleMethodGeneratorService = new SingleMethodGeneratorService();
            var compoundMethodGeneratorFactory = new CompoundMethodGeneratorFactory(singleMethodGeneratorService);
            var existingMethodControlFactory = new ExistingMethodControlFactory();
            var mappingInformationService = new MappingInformationService(compoundMethodGeneratorFactory, existingMethodControlFactory);
            var mappingGeneratorService = new MappingGeneratorService(mappingInformationService);

            return new MapThisCodeRefactoringProvider(mappingGeneratorService);
        }

        private static MemberDataSerializer<TestDataDto> GetData(string before, string refactored)
        {
            return new MemberDataSerializer<TestDataDto>(new TestDataDto() { Before = before, Refactored = refactored });
        }

    }
}
