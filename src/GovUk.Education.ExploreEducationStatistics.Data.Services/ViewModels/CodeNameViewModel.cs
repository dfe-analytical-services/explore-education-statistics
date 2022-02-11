#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record CodeNameViewModel
    {
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
