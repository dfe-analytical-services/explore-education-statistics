using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class CsvUtil
    {
        public static T BuildType<T>(IReadOnlyList<string> rowValues, List<string> colValues, string column,
            Func<string, T> func)
        {
            var value = Value(rowValues, colValues, column);
            return value == null ? default(T) : func(value);
        }

        public static T BuildType<T>(IReadOnlyList<string> rowValues, List<string> colValues, IEnumerable<string> columns,
            Func<string[], T> func)
        {
            var values = Values(rowValues, colValues, columns);
            return values.All(value => value == null) ? default(T) : func(values);
        }

        public static string[] Values(IReadOnlyList<string> rowValues, List<string> colValues, IEnumerable<string> columns)
        {
            return columns.Select(c => Value(rowValues, colValues, c)).ToArray();
        }

        public static string Value(IReadOnlyList<string> rowValues, List<string> colValues, string column)
        {
            return colValues.Contains(column) ? rowValues[colValues.FindIndex(h => h.Equals(column))].Trim().NullIfWhiteSpace() : null;
        }

        public static List<string> GetColumnValues(DataColumnCollection cols)
        {
            return cols.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        }

        public static List<string> GetRowValues(DataRow row)
        {
            return row.ItemArray.Select(x => x.ToString()).ToList();
        }

        public static GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var value = Value(rowValues, colValues, "geographic_level");
            foreach (var val in (GeographicLevel[]) Enum.GetValues(typeof(GeographicLevel)))
            {
                if (val.GetEnumLabel().ToLower().Equals(value.ToLower()))
                {
                    return val;
                }
            }

            throw new InvalidGeographicLevelException(value);
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored. 
        /// </summary>
        public static bool IsRowAllowed(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return soleGeographicLevel ||
                   !GetGeographicLevel(rowValues, colValues).IsSoloImportableLevel();
        }
    }
}
