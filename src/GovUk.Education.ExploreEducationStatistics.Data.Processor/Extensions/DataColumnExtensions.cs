using System;
using System.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions
{
    public static class DataColumnExtensions
    {
        public static DataColumn CopyTo(this DataColumn column, DataTable table)
        {
            var newColumn = new DataColumn(column.ColumnName, column.DataType, column.Expression, column.ColumnMapping)
            {
                AllowDBNull = column.AllowDBNull,
                AutoIncrement = column.AutoIncrement,
                AutoIncrementSeed = column.AutoIncrementSeed,
                AutoIncrementStep = column.AutoIncrementStep,
                Caption = column.Caption,
                DateTimeMode = column.DateTimeMode,
                DefaultValue = column.DefaultValue,
                MaxLength = column.MaxLength,
                ReadOnly = column.ReadOnly,
                Unique = column.Unique
            };

            table.Columns.Add(newColumn);

            return newColumn;
        }

        public static DataColumn CopyColumnTo(this DataTable sourceTable, string columnName, DataTable destinationTable)
        {
            if (sourceTable.Columns.Contains(columnName))
            {
                return sourceTable.Columns[columnName].CopyTo(destinationTable);
            }
            else
            {
                throw new ArgumentException("The specified column does not exist", "columnName");
            }
        }
    }
}