#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreeNode : BasePublicationTreeNode
    {
        public string? LegacyPublicationUrl { get; init; }
    }
}
