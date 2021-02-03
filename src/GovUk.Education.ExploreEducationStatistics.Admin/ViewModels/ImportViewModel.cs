using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ImportViewModel
    {
        public List<string> Errors { get; set; }
        public int PercentageComplete { get; set; }
        public int StagePercentageComplete { get; set; }
        public int NumberOfRows { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ImportStatus Status { get; set; }

        public static ImportViewModel NotFound()
        {
            return new ImportViewModel
            {
                Status = ImportStatus.NOT_FOUND
            };
        }
    }
}
