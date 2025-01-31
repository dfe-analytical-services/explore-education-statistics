using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleasePublishingStatusViewModel
    {
        public Guid ReleaseId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleasePublishingStatusContentStage ContentStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleasePublishingStatusFilesStage FilesStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleasePublishingStatusPublishingStage PublishingStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleasePublishingStatusOverallStage OverallStage { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
