using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class QueryUtil
    {
        public static Dictionary<string, string> FilterAttributes(
            Dictionary<string, string> attributes,
            ICollection<string> filter)
        {
            return (
                from kvp in attributes
                where filter.Contains(kvp.Key)
                select kvp
            ).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}