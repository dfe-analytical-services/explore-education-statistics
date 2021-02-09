using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleaseStatusViewModel
    {
        public Guid ReleaseId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatusDataStage DataStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatusContentStage ContentStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatusFilesStage FilesStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatusPublishingStage PublishingStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatusOverallStage OverallStage { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}