using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class TimeIdentifierCategoryModel
    {
        [JsonConverter(typeof(TimeIdentifierCategoryJsonConverter))]
        public TimeIdentifierCategory Category { get; set; }
        public List<TimeIdentifierModel> TimeIdentifiers { get; set; }
    }
}