using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public class TimePeriodQuery
    {
        public int StartYear { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier StartCode { get; set; }

        public int EndYear { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier EndCode { get; set; }

        public TimePeriodQuery()
        {
        }

        public TimePeriodQuery(int startYear, TimeIdentifier startCode, int endYear, TimeIdentifier endCode)
        {
            StartYear = startYear;
            StartCode = startCode;
            EndYear = endYear;
            EndCode = endCode;
        }

        public TimePeriodQuery Clone()
        {
            return (TimePeriodQuery)MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{nameof(StartYear)}: {StartYear}, " +
                   $"{nameof(StartCode)}: {StartCode.GetEnumValue()}, " +
                   $"{nameof(EndYear)}: {EndYear}, " +
                   $"{nameof(EndCode)}: {EndCode.GetEnumValue()}";
        }
    }
}
