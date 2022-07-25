using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class DataImportViewModel
    {
        public List<string> Errors { get; set; }
        public int PercentageComplete { get; set; }
        public int StagePercentageComplete { get; set; }
        public int TotalRows { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DataImportStatus Status { get; set; }

        public static DataImportViewModel NotFound()
        {
            return new DataImportViewModel
            {
                Status = DataImportStatus.NOT_FOUND
            };
        }
    }
}
