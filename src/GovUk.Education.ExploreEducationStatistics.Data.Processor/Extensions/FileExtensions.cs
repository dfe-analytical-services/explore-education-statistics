using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions
{
    public static class FileExtensions
    {
        public static DataTable GetCsvTable(this SubjectData subjectData)
        {
            return GetDataTableFromBlob(subjectData.DataBlob).Result;
        }

        public static DataTable GetMetaTable(this SubjectData subjectData)
        {
            return GetDataTableFromBlob(subjectData.MetaBlob).Result;
        }

        private static async Task<DataTable> GetDataTableFromBlob(CloudBlob blob)
        {
            var reader = new StreamReader(await blob.OpenReadAsync());
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            using var dr = new CsvDataReader(csv);
            var dt = new DataTable();
            dt.Load(dr);
            return dt;
        }
    }
}