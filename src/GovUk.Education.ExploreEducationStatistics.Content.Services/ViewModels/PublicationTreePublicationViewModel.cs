#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreePublicationViewModel
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public PublicationType Type { get; set; }

        public bool IsSuperseded { get; set; }

        public bool LatestReleaseHasData { get; set; }

        public bool AnyLiveReleaseHasData { get; set; }
    }

    public enum PublicationType
    {
        NationalAndOfficial,
        AdHoc,
        Experimental,
        ManagementInformation
    }
}
