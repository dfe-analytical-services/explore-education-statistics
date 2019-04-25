using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public static class CsvUtil
    {
        public static T BuildType<T>(IReadOnlyList<string> line, List<string> headers, IEnumerable<string> columns,
            Func<string[], T> func)
        {
            var values = Values(line, headers, columns);
            return values.All(value => value == null) ? default(T) : func(values);
        }

        public static string[] Values(IReadOnlyList<string> line, List<string> headers, IEnumerable<string> columns)
        {
            return columns.Select(c => Value(line, headers, c)).ToArray();
        }

        public static string Value(IReadOnlyList<string> line, List<string> headers, string column)
        {
            return headers.Contains(column) ? 
                line[headers.FindIndex(h => h.Equals(column))].NullIfWhiteSpace() : null;
        }
    }
}