#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreeNode : BasePublicationTreeNode
    {
        public string? LegacyPublicationUrl { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublicationType Type { get; set; }
    }

    public enum PublicationType
    {
        NationalAndOfficial,
        AdHoc,
        Experimental,
        ManagementInformation,
        Legacy
    }
}
