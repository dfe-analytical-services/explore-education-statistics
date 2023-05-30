#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    // TODO EES-3755 Remove after Permalink snapshot work is complete
    public record CodeNameViewModel
    {
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
