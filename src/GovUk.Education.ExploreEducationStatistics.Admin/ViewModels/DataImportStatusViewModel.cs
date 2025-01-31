using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class DataImportStatusViewModel
    {
        public List<string> Errors { get; set; }
        public int PercentageComplete { get; set; }
        public int StagePercentageComplete { get; set; }
        public int? TotalRows { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DataImportStatus Status { get; set; }

        public static DataImportStatusViewModel NotFound()
        {
            return new DataImportStatusViewModel
            {
                Status = DataImportStatus.NOT_FOUND
            };
        }
    }
}
