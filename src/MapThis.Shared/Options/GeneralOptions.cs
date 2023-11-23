using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MapThis.Vsix.Options
{
    [Guid("9250c34f-46bd-4f0e-ac23-bf27faa6ad87")]
    public class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("General")]
        [DisplayName("Use pattern matching for null checking")]
        [Description("Use \"x is null\" (recommended) instead of \"x == null.\".")]
        [TypeConverter(typeof(EnabledDisabledConverter))]
        [DefaultValue(true)]
        public bool UsePatternMatchingForNullChecking { get; set; } = true;
    }
}