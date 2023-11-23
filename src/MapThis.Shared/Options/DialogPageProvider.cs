using OptionsSample.Options;

namespace MapThis.Vsix.Options
{
    /// <summary>
    /// A provider for custom <see cref="DialogPage" /> implementations.
    /// </summary>
    public class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> { }
    }
}
