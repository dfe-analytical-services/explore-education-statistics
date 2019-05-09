using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions
{
    public static class DataTableExtension
    {
        public static DataTable AsIdListTable<T>(this IEnumerable<T> idList)
        {
            var dataTable = new DataTable();
            var idColumn = new DataColumn("id");

            dataTable.Columns.Add(idColumn);
            idList?.ToList().ForEach(filter => { AddNewRow(dataTable, idColumn, filter); });

            return dataTable;
        }

        private static void AddNewRow<T>(DataTable dataTable, DataColumn idColumn, T filter)
        {
            var row = dataTable.NewRow();
            row[idColumn] = filter;
            dataTable.Rows.Add(row);
        }
    }
}