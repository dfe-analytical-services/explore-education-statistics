using System.Data;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class DataTableUtils
    {
        public static DataTable CreateFromStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            using var dr = new CsvDataReader(csv);

            var dt = new DataTable();
            dt.Load(dr);

            return dt;
        }
    }
}