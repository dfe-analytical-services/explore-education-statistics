#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public enum PermalinkStatus
    {
        Current,
        SubjectRemoved,
        SubjectReplacedOrRemoved,
        NotForLatestRelease,
        PublicationSuperseded
    }

    public class LegacyPermalinkViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PermalinkStatus Status { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }
    }
}
