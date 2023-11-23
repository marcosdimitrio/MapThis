using ExperimentalTools.Tests.Infrastructure.Refactoring;
using MapThis.Tests.Builder;
using MapThis.Tests.Factories;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MapThis.Tests.TestSuites.PositionalRecordTests
{
    public class PositionalRecordTestSuite : RefactoringTest
    {
        public PositionalRecordTestSuite(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> GetClients()
        {
            yield return new object[] { "01 Should map from a record to a positional record", true, 0, GetData(Resources.PositionalRecord_01_Before, Resources.PositionalRecord_01_Refactored) };
            yield return new object[] { "02 Should map from a record to a positional record when destination doesn't have one of the parameters", true, 0, GetData(Resources.PositionalRecord_02_Before, Resources.PositionalRecord_02_Refactored) };
            yield return new object[] { "03 Should map from a record to a positional record with a List of simple type", true, 0, GetData(Resources.PositionalRecord_03_Before, Resources.PositionalRecord_03_Refactored) };
            yield return new object[] { "04 Should map with null check for positional record", true, 1, GetData(Resources.PositionalRecord_04_Before, Resources.PositionalRecord_04_Refactored) };
            yield return new object[] { "05 Should not map a record when it has a mix of positional properties and normal properties", false, 0, GetData(Resources.PositionalRecord_05_Before, null) };
            yield return new object[] { "06 Should map from a class to a positional record", true, 0, GetData(Resources.PositionalRecord_06_Before, Resources.PositionalRecord_06_Refactored) };
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
