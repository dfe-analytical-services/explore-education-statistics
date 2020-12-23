using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ImportStatusBau
    {
        public string SubjectTitle { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid PublicationId { get; set; }
        public string PublicationTitle { get; set; }
        public Guid ReleaseId { get; set; }
        public string ReleaseTitle { get; set; }
        public string DataFileName { get; set; }
        public int NumberOfRows { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public IStatus Status { get; set; }
        public int StagePercentageComplete { get; set; }
    }
}
