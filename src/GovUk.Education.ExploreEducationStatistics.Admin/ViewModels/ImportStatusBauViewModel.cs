using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ImportStatusBauViewModel
    {
        public string SubjectTitle { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid PublicationId { get; set; }
        public string PublicationTitle { get; set; }
        public Guid ReleaseId { get; set; }
        public string ReleaseTitle { get; set; }
        public Guid FileId { get; set; }
        public string DataFileName { get; set; }
        public int? TotalRows { get; set; }
        public int Batches { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DataImportStatus Status { get; set; }
        public int StagePercentageComplete { get; set; }
        public int PercentageComplete { get; set; }
    }
}
