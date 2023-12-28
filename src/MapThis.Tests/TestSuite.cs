using ExperimentalTools.Tests.Infrastructure.Refactoring;
using MapThis.Tests.Builder;
using MapThis.Tests.Factories;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MapThis.Tests
{
    public class TestSuite : RefactoringTest
    {
        public TestSuite(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> GetClients()
        {
            yield return new object[] { "01 Should map a simple class with one parameter", true, 0, GetData(Resources._01_Before, Resources._01_Refactored) };
            yield return new object[] { "04 Should automatically add usings for classes", true, 0, GetData(Resources._04_Before, Resources._04_Refactored) };
            yield return new object[] { "05 Should automatically add usings for collections", true, 0, GetData(Resources._05_Before, Resources._05_Refactored) };
            yield return new object[] { "08 Should keep static access modifier if it's present for every child method", true, 0, GetData(Resources._08_Before, Resources._08_Refactored) };
            yield return new object[] { "09 Should create all new methods for classes or colletions as private", true, 0, GetData(Resources._09_Before, Resources._09_Refactored) };
            yield return new object[] { "10 Should not create foreach variable with the same name as the first parameter", true, 0, GetData(Resources._10_Before, Resources._10_Refactored) };
            yield return new object[] { "11 Should not create foreach variable with the same name as other method's properties", true, 0, GetData(Resources._11_Before, Resources._11_Refactored) };
            yield return new object[] { "12 Should not create foreach variable when defaults are taken by properties", true, 0, GetData(Resources._12_Before, Resources._12_Refactored) };
            yield return new object[] { "13 Should not create variable (newItem) with the same name as other method's properties", true, 0, GetData(Resources._13_Before, Resources._13_Refactored) };
            yield return new object[] { "14 Should not create variable (destination) with the same name as other method's properties", true, 0, GetData(Resources._14_Before, Resources._14_Refactored) };
            yield return new object[] { "16 Should use IList for new list map methods instead of List", true, 0, GetData(Resources._16_Before, Resources._16_Refactored) };
            yield return new object[] { "17 Should map with null check for classes", true, 1, GetData(Resources._17_Before, Resources._17_Refactored) };
            yield return new object[] { "18 Should map with null check for collections", true, 1, GetData(Resources._18_Before, Resources._18_Refactored) };
            yield return new object[] { "21 Should not repeat mappings when Parent has GrandChild and List of Child that has GrandChild", true, 0, GetData(Resources._21_Before, Resources._21_Refactored) };
            yield return new object[] { "22 Should stop mapping at class mapping that already exist on class", true, 0, GetData(Resources._22_Before, Resources._22_Refactored) };
            yield return new object[] { "23 Should keep \"namespace name\" like in Parents.Parent and not add to usings", true, 0, GetData(Resources._23_Before, Resources._23_Refactored) };
            yield return new object[] { "24 Should map simple types non nullable to nullable directly", true, 0, GetData(Resources._24_Before, Resources._24_Refactored) };
            yield return new object[] { "25 Should map simple types nullable to non nullable directly", true, 0, GetData(Resources._25_Before, Resources._25_Refactored) };
            yield return new object[] { "28 Should keep using namespace alias for classes", true, 0, GetData(Resources._28_Before, Resources._28_Refactored) };
            yield return new object[] { "29 Should keep using namespace alias for collections", true, 0, GetData(Resources._29_Before, Resources._29_Refactored) };
            yield return new object[] { "32 Should map when return type is a generic class", true, 0, GetData(Resources._32_Before, Resources._32_Refactored) };
            yield return new object[] { "33 Should map when first parameter type is a generic class", true, 0, GetData(Resources._33_Before, Resources._33_Refactored) };
            yield return new object[] { "34 Should keep namespace alias in types for further methods", true, 0, GetData(Resources._34_Before, Resources._34_Refactored) };
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
            yield return new object[] { "48 Should map properties that are arrays of the same simple type directly", true, 0, GetData(Resources._48_Before, Resources._48_Refactored) };
            yield return new object[] { "50 Should map one enum to another enum", true, 0, GetData(Resources._50_Before, Resources._50_Refactored) };
            yield return new object[] { "51 Should map property directly when it doesn't exist in Source type", true, 0, GetData(Resources._51_Before, Resources._51_Refactored) };
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
            yield return new object[] { "67 Should map a class when it has many simple type properties", true, 0, GetData(Resources._67_Before, Resources._67_Refactored) };
            yield return new object[] { "68 Should map a class when the child class has many simple type properties", true, 0, GetData(Resources._68_Before, Resources._68_Refactored) };
            yield return new object[] { "69 Should map a class when it has two child classes", true, 0, GetData(Resources._69_Before, Resources._69_Refactored) };
            yield return new object[] { "70 Should map a class when it has many enum properties", true, 0, GetData(Resources._70_Before, Resources._70_Refactored) };
            yield return new object[] { "71 Should map a class when the child class has many enum properties", true, 0, GetData(Resources._71_Before, Resources._71_Refactored) };
            yield return new object[] { "72 Should map a class when it has many list properties", true, 0, GetData(Resources._72_Before, Resources._72_Refactored) };
            yield return new object[] { "73 Should map a class when the child class has many list properties", true, 0, GetData(Resources._73_Before, Resources._73_Refactored) };
            yield return new object[] { "74 Should not map a child class when it is an interface", true, 0, GetData(Resources._74_Before, Resources._74_Refactored) };
            yield return new object[] { "75 Should map a class when properties order is different", true, 0, GetData(Resources._75_Before, Resources._75_Refactored) };
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD200", Justification = "The name is displayed in test explorer")]
        #endregion
        public Task Test(string name, bool shouldRefactor, int refactoringIndex, MemberDataSerializer<TestDataDto> dto)
        {
            var refactoringText = refactoringIndex == 0 ? "Map this" : "Map this with null check";
            if (shouldRefactor)
            {
                return RunMultipleActionsTestAsync(refactoringText, dto.Object.Before, dto.Object.Refactored);
            }

            return RunNoActionTestAsync(dto.Object.Before);
        }

        protected override CodeRefactoringProvider Provider => ProviderFactory.GetCodeRefactoringProvider();

        private static MemberDataSerializer<TestDataDto> GetData(string before, string refactored)
        {
            return new MemberDataSerializer<TestDataDto>(new TestDataDto() { Before = before, Refactored = refactored });
        }
    }
}
