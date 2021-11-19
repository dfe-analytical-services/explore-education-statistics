#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class LegendOptionsMetaValueModel<TOptions>
    {
        public string? Hint { get; set; }
        public string? Legend { get; set; }
        public TOptions Options { get; set; }
        public string? Name { get; set; }
    }
}
