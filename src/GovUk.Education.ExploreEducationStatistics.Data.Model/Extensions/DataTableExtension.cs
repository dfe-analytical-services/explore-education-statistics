using System.Collections.Generic;
using System.Data;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions
{
    public static class DataTableExtension
    {
        public static DataTable AsIdListTable<T>(this IEnumerable<T> values)
        {
            var dataTable = new DataTable();
            var column = new DataColumn("id");

            dataTable.Columns.Add(column);

            var rows = values?.Select(value => new[]
            {
                new ColumnValue(column, value)
            }).ToMultidimensionalArray();

            AddRows(dataTable, rows);

            return dataTable;
        }

        public static DataTable AsTimePeriodListTable(
            this IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> values)
        {
            var dataTable = new DataTable();
            var yearColumn = new DataColumn("year");
            var timeIdentifierColumn = new DataColumn("timeIdentifier");

            dataTable.Columns.Add(yearColumn);
            dataTable.Columns.Add(timeIdentifierColumn);

            var rows = values.Select(value => new[]
            {
                new ColumnValue(yearColumn, value.Year),
                new ColumnValue(timeIdentifierColumn, value.TimeIdentifier.GetEnumValue())
            }).ToMultidimensionalArray();

            AddRows(dataTable, rows);

            return dataTable;
        }

        private static void AddRows(DataTable dataTable, ColumnValue[,] array)
        {
            if (array == null)
            {
                return;
            }

            for (var i = 0; i < array.GetLength(0); i++)
            {
                var row = dataTable.NewRow();
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    var x = array[i, j];
                    row[x.Column] = x.Value;
                }
                dataTable.Rows.Add(row);
            }
        }

        private class ColumnValue
        {
            public DataColumn Column { get; }
            public object Value { get; }

            public ColumnValue(DataColumn column, object value)
            {
                Column = column;
                Value = value;
            }
        }
    }
}