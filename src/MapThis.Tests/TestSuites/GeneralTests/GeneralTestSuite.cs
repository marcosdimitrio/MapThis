using ExperimentalTools.Tests.Infrastructure.Refactoring;
using MapThis.Tests.Builder;
using MapThis.Tests.Factories;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MapThis.Tests.TestSuites.GeneralTests
{
    public class GeneralTestSuite : RefactoringTest
    {
        public GeneralTestSuite(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> GetClients()
        {
            yield return new object[] { "01 Should not exclude additional parameters from first method", true, 0, GetData(Resources.General_01_Before, Resources.General_01_Refactored) };
            yield return new object[] { "02 Should keep accessibility of first method", true, 0, GetData(Resources.General_02_Before, Resources.General_02_Refactored) };
            yield return new object[] { "03 Should not add usings for namespaces above the current one", true, 0, GetData(Resources.General_03_Before, Resources.General_03_Refactored) };
            yield return new object[] { "04 Should keep original attributes on the first method", true, 0, GetData(Resources.General_04_Before, Resources.General_04_Refactored) };
            yield return new object[] { "05 Should place new methods below public methods in between the first map method", true, 0, GetData(Resources.General_05_Before, Resources.General_05_Refactored) };
            yield return new object[] { "06 Should not create map method from list to class", false, 0, GetData(Resources.General_06_Before, null) };
            yield return new object[] { "07 Should not create map method from class to list", false, 0, GetData(Resources.General_07_Before, null) };
            yield return new object[] { "08 Should show map options when cursor is at the return type", true, 0, GetData(Resources.General_08_Before, Resources.General_08_Refactored) };
            yield return new object[] { "09 Should show map options when cursor is at opening parenthesis", true, 0, GetData(Resources.General_09_Before, Resources.General_09_Refactored) };
            yield return new object[] { "10 Should not map method when return type is broken", false, 0, GetData(Resources.General_10_Before, null) };
            yield return new object[] { "11 Should not map method when first parameter type is broken", false, 0, GetData(Resources.General_11_Before, null) };
            yield return new object[] { "12 Should not map abstract methods", false, 0, GetData(Resources.General_12_Before, null) };
            yield return new object[] { "13 Should not map method when return is void", false, 0, GetData(Resources.General_13_Before, null) };
            yield return new object[] { "14 Should not map when return type is a string (simple type)", false, 0, GetData(Resources.General_14_Before, null) };
            yield return new object[] { "15 Should not map when source parameter is an interface", false, 0, GetData(Resources.General_15_Before, null) };
            yield return new object[] { "16 Should not map when destination type is an interface", false, 0, GetData(Resources.General_16_Before, null) };
            yield return new object[] { "17 Should not map method in an interface", false, 0, GetData(Resources.General_17_Before, null) };
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
        public Task Test_Method(string name, bool shouldRefactor, int refactoringIndex, MemberDataSerializer<TestDataDto> dto)
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
