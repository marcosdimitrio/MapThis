using MapThis.Refactorings.MappingGenerator;
using MapThis.Services.EnumMethodGenerator;
using MapThis.Services.ExistingMethodsControl.Factories;
using MapThis.Services.MappingInformation;
using MapThis.Services.MethodGenerator.Factories;
using MapThis.Services.SingleMethodGenerator;
using MapThis.Tests.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using RoslynTestKit;
using System.Collections.Generic;
using System.ComponentModel;
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
            yield return new object[] { "08 Should keep static access modifier if it's present for every child method", true, 0, GetData(Resources._08_Before, Resources._08_Refactored) };
            yield return new object[] { "09 Should create all new methods for classes or colletions as private", true, 0, GetData(Resources._09_Before, Resources._09_Refactored) };
            yield return new object[] { "10 Should not create foreach variable with the same name as the first parameter", true, 0, GetData(Resources._10_Before, Resources._10_Refactored) };
            yield return new object[] { "11 Should not create foreach variable with the same name as other method's properties", true, 0, GetData(Resources._11_Before, Resources._11_Refactored) };
            yield return new object[] { "12 Should not create foreach variable when defaults are taken by properties", true, 0, GetData(Resources._12_Before, Resources._12_Refactored) };
            yield return new object[] { "13 Should not create variable (newItem) with the same name as other method's properties", true, 0, GetData(Resources._13_Before, Resources._13_Refactored) };
            yield return new object[] { "14 Should not create variable (destination) with the same name as other method's properties", true, 0, GetData(Resources._14_Before, Resources._14_Refactored) };
            yield return new object[] { "15 Should place new methods below public methods in between the first map method", true, 0, GetData(Resources._15_Before, Resources._15_Refactored) };
            yield return new object[] { "16 Should use IList for new list map methods instead of List", true, 0, GetData(Resources._16_Before, Resources._16_Refactored) };
            yield return new object[] { "17 Should map with null check for classes", true, 1, GetData(Resources._17_Before, Resources._17_Refactored) };
            yield return new object[] { "18 Should map with null check for collections", true, 1, GetData(Resources._18_Before, Resources._18_Refactored) };
            yield return new object[] { "19 Should not create map method from list to class", false, 0, GetData(Resources._19_Before, null) };
            yield return new object[] { "20 Should not create map method from class to list", false, 0, GetData(Resources._20_Before, null) };
            yield return new object[] { "21 Should not repeat mappings when Parent has GrandChild and List of Child that has GrandChild", true, 0, GetData(Resources._21_Before, Resources._21_Refactored) };
            yield return new object[] { "22 Should stop mapping at class mapping that already exist on class", true, 0, GetData(Resources._22_Before, Resources._22_Refactored) };
            yield return new object[] { "23 Should keep \"namespace name\" like in Parents.Parent and not add to usings", true, 0, GetData(Resources._23_Before, Resources._23_Refactored) };
            yield return new object[] { "24 Should map non nullable to nullable directly", true, 0, GetData(Resources._24_Before, Resources._24_Refactored) };
            yield return new object[] { "25 Should map nullable to non nullable directly", true, 0, GetData(Resources._25_Before, Resources._25_Refactored) };
            yield return new object[] { "26 Should show map options when cursor is at the return type", true, 0, GetData(Resources._26_Before, Resources._26_Refactored) };
            yield return new object[] { "27 Should show map options when cursor is at opening parenthesis", true, 0, GetData(Resources._27_Before, Resources._27_Refactored) };
            yield return new object[] { "28 Should keep using namespace alias for classes", true, 0, GetData(Resources._28_Before, Resources._28_Refactored) };
            yield return new object[] { "29 Should keep using namespace alias for collections", true, 0, GetData(Resources._29_Before, Resources._29_Refactored) };
            yield return new object[] { "30 Should not map method when return type is broken", false, 0, GetData(Resources._30_Before, null) };
            yield return new object[] { "31 Should not map method when first parameter type is broken", false, 0, GetData(Resources._31_Before, null) };
            yield return new object[] { "32 Should map when return type is a generic class", true, 0, GetData(Resources._32_Before, Resources._32_Refactored) };
            yield return new object[] { "33 Should map when first parameter type is a generic class", true, 0, GetData(Resources._33_Before, Resources._33_Refactored) };
            yield return new object[] { "34 Should keep namespace alias in types for further methods", true, 0, GetData(Resources._34_Before, Resources._34_Refactored) };
            yield return new object[] { "35 Should not map abstract methods", false, 0, GetData(Resources._35_Before, null) };
            yield return new object[] { "36 Should map a class array", true, 0, GetData(Resources._36_Before, Resources._36_Refactored) };
            yield return new object[] { "37 Should map a simple type array", true, 0, GetData(Resources._37_Before, Resources._37_Refactored) };
            yield return new object[] { "38 Should map a list of simple type", true, 0, GetData(Resources._38_Before, Resources._38_Refactored) };
            yield return new object[] { "39 Should map between IList and ICollection", true, 0, GetData(Resources._39_Before, Resources._39_Refactored) };
            yield return new object[] { "40 Should map between IList and IEnumerable", true, 0, GetData(Resources._40_Before, Resources._40_Refactored) };
            yield return new object[] { "41 Should map between ICollection and IList", true, 0, GetData(Resources._41_Before, Resources._41_Refactored) };
            yield return new object[] { "42 Should map between ICollection and ICollection", true, 0, GetData(Resources._42_Before, Resources._42_Refactored) };
            yield return new object[] { "43 Should map between ICollection and IEnumerable", true, 0, GetData(Resources._43_Before, Resources._43_Refactored) };
            yield return new object[] { "44 Should map between IEnumerable and IList", true, 0, GetData(Resources._44_Before, Resources._44_Refactored) };
            yield return new object[] { "45 Should map between IEnumerable and ICollection", true, 0, GetData(Resources._45_Before, Resources._45_Refactored) };
            yield return new object[] { "46 Should map between IEnumerable and IEnumerable", true, 0, GetData(Resources._46_Before, Resources._46_Refactored) };
            yield return new object[] { "47 Should not map method when return is void", false, 0, GetData(Resources._47_Before, null) };
            yield return new object[] { "48 Should map properties that are arrays of the same simple type directly", true, 0, GetData(Resources._48_Before, Resources._48_Refactored) };
            yield return new object[] { "49 Should not map when return type is a string (simple type)", false, 0, GetData(Resources._49_Before, null) };
            yield return new object[] { "50 Should map one enum to another enum", true, 0, GetData(Resources._50_Before, Resources._50_Refactored) };
            yield return new object[] { "51 Should map property directly when it doesn't exist in Source type", true, 0, GetData(Resources._51_Before, Resources._51_Refactored) };
            yield return new object[] { "52 Should not map when source parameter is an interface", false, 0, GetData(Resources._52_Before, null) };
            yield return new object[] { "53 Should not map when destination type is an interface", false, 0, GetData(Resources._53_Before, null) };
            yield return new object[] { "54 Should not map method in an interface", false, 0, GetData(Resources._54_Before, null) };
            yield return new object[] { "55 Should map from a record to a record", true, 0, GetData(Resources._55_Before, Resources._55_Refactored) };
            yield return new object[] { "56 Should map from a record to a class", true, 0, GetData(Resources._56_Before, Resources._56_Refactored) };
            yield return new object[] { "57 Should map from a class to a record", true, 0, GetData(Resources._57_Before, Resources._57_Refactored) };
            yield return new object[] { "58 Should map an enum when it's inside a parent class", true, 0, GetData(Resources._58_Before, Resources._58_Refactored) };
            yield return new object[] { "59 Should map a list of enum into another list of enum", true, 0, GetData(Resources._59_Before, Resources._59_Refactored) };
            yield return new object[] { "60 Should keep \"namespace name\" like in Parents.Parent and not add to usings for enums", true, 0, GetData(Resources._60_Before, Resources._60_Refactored) };
            yield return new object[] { "61 Should keep using namespace alias for enums", true, 0, GetData(Resources._61_Before, Resources._61_Refactored) };
            yield return new object[] { "62 Should map enum item directly when it doesn't exist in Source enum", true, 0, GetData(Resources._62_Before, Resources._62_Refactored) };
            yield return new object[] { "63 Should map a list of enum when it's inside a parent class", true, 0, GetData(Resources._63_Before, Resources._63_Refactored) };
            yield return new object[] { "64 Should stop mapping when enum list already exists", true, 0, GetData(Resources._64_Before, Resources._64_Refactored) };
            yield return new object[] { "65 Should stop mapping when enum mapping already exist when mapping from a list", true, 0, GetData(Resources._65_Before, Resources._65_Refactored) };
            yield return new object[] { "66 Should stop mapping when enum mapping already exist when mapping from a class", true, 0, GetData(Resources._66_Before, Resources._66_Refactored) };
        }

        /// <summary>
        /// Executes all tests
        /// </summary>
        /// <param name="name">Name of the test</param>
        /// <param name="shouldRefactor">Determines whether there should be a code refactoring in this context</param>
        /// <param name="refactoringIndex">Determines which CodeAction was selected, for instance "Map this" or "Map this with null check"</param>
        /// <param name="dto">The test source code, with before and refactored code</param>
        [Theory]
        [MemberData(nameof(GetClients))]
        #region SuppressMessage
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
            var enumMethodGenerator = new EnumMethodGenerator();
            var compoundMethodGeneratorFactory = new CompoundMethodGeneratorFactory(singleMethodGeneratorService, enumMethodGenerator);
            var existingMethodControlFactory = new ExistingMethodControlFactory();
            var mappingInformationService = new MappingInformationService(compoundMethodGeneratorFactory, existingMethodControlFactory);
            var mappingGeneratorService = new MappingGeneratorService(mappingInformationService);

            return new MapThisCodeRefactoringProvider(mappingGeneratorService);
        }

        protected override IReadOnlyCollection<MetadataReference> References
        {
            get
            {
                return new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(InvalidEnumArgumentException).Assembly.Location)
                };
            }
        }

        private static MemberDataSerializer<TestDataDto> GetData(string before, string refactored)
        {
            return new MemberDataSerializer<TestDataDto>(new TestDataDto() { Before = before, Refactored = refactored });
        }

    }
}
