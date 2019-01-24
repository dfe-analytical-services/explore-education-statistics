using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class Levels
    {
        private static readonly Dictionary<Level, string> values = new Dictionary<Level, string>
        {
            {Level.National, "National"},
            {Level.Local_Authority, "LOCAL AUTHORITY"}
        };

        public static string getStringFromEnum(Level value)
        {
            return values.GetValueOrDefault(value);
        }
    }
}