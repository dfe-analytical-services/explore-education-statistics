using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public static class QueryUtil
    {
        public static Dictionary<string, string> FilterMeasures(
            Dictionary<string, string> measures,
            IEnumerable<string> indicators)
        {
            return (
                from kvp in measures
                where indicators.Contains(kvp.Key)
                select kvp
            ).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}