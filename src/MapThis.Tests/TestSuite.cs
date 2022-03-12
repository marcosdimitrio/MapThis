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
            yield return new object[] { "01 Should map a simple class with one parameter", true, 0, GetData(Resources._01_Before, Resources._01_Refactored) };
            yield return new object[] { "02 Should not exclude additional parameters from first method", true, 0, GetData(Resources._02_Before, Resources._02_Refactored) };
            yield return new object[] { "03 Should keep accessibility of first method", true, 0, GetData(Resources._03_Before, Resources._03_Refactored) };
            yield return new object[] { "04 Should automatically add usings for classes", true, 0, GetData(Resources._04_Before, Resources._04_Refactored) };
            yield return new object[] { "05 Should automatically add usings for collections", true, 0, GetData(Resources._05_Before, Resources._05_Refactored) };
            yield return new object[] { "06 Should not add usings for namespaces above the current one", true, 0, GetData(Resources._06_Before, Resources._06_Refactored) };
            yield return new object[] { "07 Should keep original attributes", true, 0, GetData(Resources._07_Before, Resources._07_Refactored) };
            yield return new object[] { "08 Should keep static access modifier if its present for every child method", true, 0, GetData(Resources._08_Before, Resources._08_Refactored) };
            yield return new object[] { "09 Should create all new methods for classes or colletions as private", true, 0, GetData(Resources._09_Before, Resources._09_Refactored) };
            yield return new object[] { "10 Should not create foreach variable with the same name as the first parameter", true, 0, GetData(Resources._10_Before, Resources._10_Refactored) };
            yield return new object[] { "11 Should not create foreach variable with the same name as other method's properties", true, 0, GetData(Resources._11_Before, Resources._11_Refactored) };
            yield return new object[] { "12 Should not create foreach variable when defaults are taken by properties", true, 0, GetData(Resources._12_Before, Resources._12_Refactored) };
            yield return new object[] { "13 Should not create variable (newItem) with the same name as other method's properties", true, 0, GetData(Resources._13_Before, Resources._13_Refactored) };
            yield return new object[] { "14 Should not create variable (destination) with the same name as other method's properties", true, 0, GetData(Resources._14_Before, Resources._14_Refactored) };
            yield return new object[] { "15 Should place new methods below public methods in between the first map method", true, 0, GetData(Resources._15_Before, Resources._15_Refactored) };
            yield return new object[] { "16 Should use IList and ICollection for new list map methods with List and Collection", true, 0, GetData(Resources._16_Before, Resources._16_Refactored) };
            yield return new object[] { "17 Should map with null check for classes", true, 1, GetData(Resources._17_Before, Resources._17_Refactored) };
            yield return new object[] { "18 Should map with null check for collections", true, 1, GetData(Resources._18_Before, Resources._18_Refactored) };
            yield return new object[] { "19 Should not create map method from list to class", false, 0, GetData(Resources._19_Before, null) };
            yield return new object[] { "20 Should not create map method from class to list", false, 0, GetData(Resources._20_Before, null) };
            yield return new object[] { "21 Should not repeat mappings when Parent has GrandChild and List of Child that has GrandChild", true, 0, GetData(Resources._21_Before, Resources._21_Refactored) };
            yield return new object[] { "22 Should stop mapping at mapping that already exist on class", true, 0, GetData(Resources._22_Before, Resources._22_Refactored) };
            yield return new object[] { "23 Should keep \"namespace name\" like in Parents.Parent and not add to usings", true, 0, GetData(Resources._23_Before, Resources._23_Refactored) };
            yield return new object[] { "24 Should map non nullable to nullable directly", true, 0, GetData(Resources._24_Before, Resources._24_Refactored) };
            yield return new object[] { "25 Should map nullable to non nullable directly", true, 0, GetData(Resources._25_Before, Resources._25_Refactored) };
            yield return new object[] { "26 Should show map options when cursor is at the return type", true, 0, GetData(Resources._26_Before, Resources._26_Refactored) };
            yield return new object[] { "27 Should show map options when cursor is at opening parenthesis", true, 0, GetData(Resources._27_Before, Resources._27_Refactored) };
            yield return new object[] { "28 Should keep using namespace alias for classes", true, 0, GetData(Resources._28_Before, Resources._28_Refactored) };
            yield return new object[] { "29 Should keep using namespace alias for collections", true, 0, GetData(Resources._29_Before, Resources._29_Refactored) };
            yield return new object[] { "30 Should not map method that has return type broken", true, 0, GetData(Resources._30_Before, Resources._30_Refactored) };
            yield return new object[] { "31 Should not map method that has first parameter type broken", true, 0, GetData(Resources._31_Before, Resources._31_Refactored) };
            yield return new object[] { "32 Should map generic classes", true, 0, GetData(Resources._32_Before, Resources._32_Refactored) };
            yield return new object[] { "33 Should keep namespace alias in types", true, 0, GetData(Resources._33_Before, Resources._33_Refactored) };
            yield return new object[] { "34 Should not map abstract methods", true, 0, GetData(Resources._34_Before, Resources._34_Refactored) };
            //Map arrays
        }

        [Theory]
        [MemberData(nameof(GetClients))]
        #region SupressMessage
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026", Justification = "The name is displayed in test explorer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0060", Justification = "The name is displayed in test explorer")]
        #endregion
        public void Test_Method(string name, bool shouldRefactor, int refactoringIndex, MemberDataSerializer<TestDataDto> dto)
        {
            if (shouldRefactor)
            {
                TestCodeRefactoring(dto.Object.Before, dto.Object.Refactored, refactoringIndex);
                return;
            }

            TestNoCodeRefactoring(dto.Object.Before);
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
