#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils
{
    public static class TableBuilderUtils
    {
        /// <summary>
        /// Calculates the maximum number of table cells that could be rendered for the result of a table query, based on
        /// the indicator, location, time period, and any other filter parameters of the query.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This works by making an assumption that data is provided for <i>all</i> combinations of location, time
        /// period and any other filter values to calculate the maximum possible count of table cells.
        /// </p>
        /// <p>
        /// The number of ways of observing each indicator is the product of the counts of the items in each of
        /// the sets of locations, time periods, and filters. This is multiplied by the count of indicators 
        /// to get the maximum possible count of table cells.
        /// </p>
        /// <p>
        /// Note that filter items have to be counted by filter since each filter adds an independent set of possibilities.
        /// </p>
        /// <p>
        /// Calculating based on attributes of the query criteria can be preferential to counting the actual result
        /// to avoid wasted overhead of executing a query that won't be rendered due to exceeding the cell count limit.
        /// </p>
        /// <p>
        /// The actual count of table cells that will be rendered is the product of the count of observation rows that
        /// match the query when it's executed and the count of indicators requested.
        /// </p>
        /// <p>
        /// There can be up to one observation for each combination of requested location, time period and other filter
        /// values where such combinations of data have been provided. Every observation contains a measurement of the
        /// requested indicators. 
        /// </p>
        /// </remarks>
        /// <param name="countOfIndicators"></param>
        /// <param name="countOfLocations"></param>
        /// <param name="countOfTimePeriods"></param>
        /// <param name="countsOfFilterItemsByFilter"></param>
        /// <returns></returns>
        public static int MaximumTableCellCount(
            int countOfIndicators,
            int countOfLocations,
            int countOfTimePeriods,
            IEnumerable<int>? countsOfFilterItemsByFilter = null)
        {
            var result = countOfIndicators * countOfLocations * countOfTimePeriods;
            if (countsOfFilterItemsByFilter != null)
            {
                foreach (var countOfFilterItems in countsOfFilterItemsByFilter)
                {
                    result *= countOfFilterItems;
                }
            }
            return result;
        }
    }
}
