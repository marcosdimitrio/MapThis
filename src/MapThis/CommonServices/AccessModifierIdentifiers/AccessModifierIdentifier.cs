using MapThis.CommonServices.AccessModifierIdentifiers.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.CommonServices.AccessModifierIdentifiers
{
    [Export(typeof(IAccessModifierIdentifier))]
    public class AccessModifierIdentifier : IAccessModifierIdentifier
    {

        public IList<SyntaxToken> GetNewMethodAccessModifiers(IList<SyntaxToken> originalModifiers)
        {
            var listToRemove = new List<SyntaxKind>();
            var listToAdd = new List<SyntaxToken>();

            if (originalModifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
            {
                listToAdd.Add(Token(SyntaxKind.PrivateKeyword));
                listToRemove.Add(SyntaxKind.PublicKeyword);
            }

            if (originalModifiers.Any(x => x.IsKind(SyntaxKind.VirtualKeyword)))
            {
                listToRemove.Add(SyntaxKind.VirtualKeyword);
            }

            var newList = listToAdd.ToList();
            newList.AddRange(originalModifiers.Where(x => !listToRemove.Contains(x.Kind())).ToList());

            return newList;
        }

    }
}
