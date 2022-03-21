#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class TableHeadersExtensions
    {
        public static List<TableHeader> FilterByType(this IEnumerable<TableHeader> tableHeaders, TableHeaderType type)
        {
            return tableHeaders.Where(header => header.Type == type).ToList();
        }
    }
}
