using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    /**
     * Converts a <see cref="DateTime"/> to and from the format yyyy-MM-dd (e.g. <c>"2008-04-12"</c>).
     */
    public class DateTimeToDateJsonConverter : IsoDateTimeConverter
    {
        public DateTimeToDateJsonConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}