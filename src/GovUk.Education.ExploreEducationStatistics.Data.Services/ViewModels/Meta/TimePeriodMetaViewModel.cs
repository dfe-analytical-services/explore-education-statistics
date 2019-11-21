using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class TimePeriodMetaViewModel
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier Code { get; set; }

        public string Label { get; set; }
        public int Year { get; set; }

        public TimePeriodMetaViewModel(int year, TimeIdentifier code)
        {
            Code = code;
            Label = TimePeriodLabelFormatter.Format(year, code);
            Year = year;
        }
    }
}