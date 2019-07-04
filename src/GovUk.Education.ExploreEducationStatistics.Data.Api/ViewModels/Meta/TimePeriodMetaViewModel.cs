using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class TimePeriodMetaViewModel
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier Code { get; set; }

        public string Label { get; set; }
        public int Year { get; set; }
    }
}