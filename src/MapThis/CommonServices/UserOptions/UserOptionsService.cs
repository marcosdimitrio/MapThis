using MapThis.CommonServices.UserOptions.Interfaces;
using MapThis.Vsix.Options;
using Microsoft.VisualStudio.Shell;
using System.Composition;

namespace MapThis.CommonServices.UserOptions
{
    [Export(typeof(IUserOptionsService))]
    public class UserOptionsService : IUserOptionsService
    {
        public GeneralOptions GeneralOptions
        {
            get
            {
#pragma warning disable VSTHRD104 // Offer async methods
                var options = ThreadHelper.JoinableTaskFactory.Run(GeneralOptions.GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
                return options;
            }
        }
    }
}
