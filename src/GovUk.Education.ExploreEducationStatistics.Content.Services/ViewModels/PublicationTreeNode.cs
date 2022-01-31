using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreeNode : BasePublicationTreeNode
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType? LatestReleaseType { get; set; }

        public string? LegacyPublicationUrl { get; init; }
    }
}
