using ExperimentalTools.Tests.Infrastructure.Refactoring;
using MapThis.Tests.Builder;
using MapThis.Tests.Factories;
using MapThis.Vsix.Options;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MapThis.Tests.TestSuites.OptionsTests
{
    public class OptionsTestSuite : RefactoringTest
    {
        private static GeneralOptions _generalOptions;

        public OptionsTestSuite(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> GetClients()
        {
            yield return new object[] { "01 Should check nulls using pattern matching", true, GetData(Resources.Options_01_Before, Resources.Options_01_Refactored) };
            yield return new object[] { "02 Should check nulls using equality operator", false, GetData(Resources.Options_02_Before, Resources.Options_02_Refactored) };
        }

        /// <summary>
        /// Executes all tests
        /// </summary>
        /// <param name="name">Name of the test</param>
        /// <param name="usePatternMatchingForNullChecking">Determines whether there to use pattern matching when checking for null</param>
        /// <param name="dto">The test source code, with before and refactored code</param>
        [Theory]
        [MemberData(nameof(GetClients))]
        #region SuppressMessage
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026", Justification = "The name is displayed in test explorer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "IDE0060", Justification = "The name is displayed in test explorer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD200", Justification = "The name is displayed in test explorer")]
        #endregion
        public Task Test_Method(string name, bool usePatternMatchingForNullChecking, MemberDataSerializer<TestDataDto> dto)
        {
            _generalOptions = new GeneralOptions()
            {
                UsePatternMatchingForNullChecking = usePatternMatchingForNullChecking,
            };

            return RunMultipleActionsTestAsync("Map this with null check", dto.Object.Before, dto.Object.Refactored);
        }

        protected override CodeRefactoringProvider Provider => ProviderFactory.GetCodeRefactoringProvider(_generalOptions);

        private static MemberDataSerializer<TestDataDto> GetData(string before, string refactored)
        {
            return new MemberDataSerializer<TestDataDto>(new TestDataDto() { Before = before, Refactored = refactored });
        }

    }
}
