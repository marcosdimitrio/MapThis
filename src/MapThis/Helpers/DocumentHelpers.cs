using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace MapThis.Helpers
{
    public static class DocumentHelpers
    {
        public static async Task<Document> ReplaceNodesAsync(this Document document, SyntaxNode oldNode, SyntaxNode newNode, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(oldNode, newNode);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
