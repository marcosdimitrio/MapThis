using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace ExperimentalTools.Tests.Infrastructure
{
    internal static class DocumentProvider
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        private const string DefaultFilePathPrefix = "Test";
        private const string CSharpDefaultFileExt = "cs";
        private const string TestProjectName = "TestProject";
        private static string[] PreExistingDocuments = { };

        public static Document[] GetDocuments(string[] sources) =>
            GetDocuments(sources, null);

        public static Document[] GetDocuments(string[] sources, string[] filePaths)
        {
            var project = CreateProject(sources, filePaths);
            var documents = project.Documents.Where(x => !PreExistingDocuments.Contains(x.Name)).ToArray();

            if (sources.Length != documents.Length)
            {
                throw new Exception("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        public static Document GetDocument(string source) =>
            CreateProject(new[] { source }, null).Documents.First();

        public static Document GetDocument(string source, string filePath) =>
            CreateProject(new[] { source }, new[] { filePath }).Documents.First();

        private static Project CreateProject(string[] sources, string[] filePaths)
        {
            if (filePaths != null && sources.Length != filePaths.Length)
            {
                throw new ArgumentException("Number of specified file paths does not match the number of sources");
            }

            if (filePaths != null && filePaths.Any(name => name == null))
            {
                throw new ArgumentException("File path can't be null");
            }

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var globalUsings = new string[] { "SharedNamespace" };

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                TestProjectName,
                TestProjectName,
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithUsings(globalUsings)
            );

            var sharedClassDocumentId = DocumentId.CreateNewId(projectId, debugName: TestProjectName);
            var globalDocumentId = DocumentId.CreateNewId(projectId, debugName: TestProjectName);

            var sharedClassSource = @"
                namespace SharedNamespace
                {
                    public class SharedClass
                    {
                        public int SharedInt { get; set; }
                    }
                    public class SharedClassDto
                    {
                        public int SharedInt { get; set; }
                    }
                }
                ";

            var documents = new[] {
                DocumentInfo.Create(
                    globalDocumentId,
                    "GlobalUsings.cs",
                    loader: TextLoader.From(
                        SourceText.From("global using SharedNamespace;").Container,
                        VersionStamp.Default)
                ),
                DocumentInfo.Create(
                    sharedClassDocumentId,
                    "SharedClass.cs",
                    loader: TextLoader.From(
                        SourceText.From(sharedClassSource).Container,
                        VersionStamp.Default)
                )
            };

            PreExistingDocuments = documents.Select(x => x.Name).ToArray();

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectInfo)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, CSharpSymbolsReference)
                .AddMetadataReference(projectId, CodeAnalysisReference)
                .AddDocuments(documents.ToImmutableArray());

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = filePaths != null
                    ? Path.GetFileName(filePaths[count])
                    : DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;

                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = filePaths != null
                    ? solution.AddDocument(documentId, newFileName, SourceText.From(source), filePath: filePaths[count])
                    : solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            return solution.GetProject(projectId);
        }
    }
}
