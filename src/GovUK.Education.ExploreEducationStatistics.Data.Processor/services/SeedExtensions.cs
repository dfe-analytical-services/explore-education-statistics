namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
    using Microsoft.WindowsAzure.Storage.Blob;

    public static class SeedExtensions
    {
        public static IEnumerable<string> GetCsvLines(this Subject subject)
        {
            return ReadAllLinesAsync(subject.CsvDataBlob).Result;
        }

        public static IEnumerable<string> GetMetaLines(this Subject subject)
        {
            return ReadAllLinesAsync(subject.CsvMetaDataBlob).Result;
        }

        private static async Task<IEnumerable<string>> ReadAllLinesAsync(CloudBlockBlob blockBlob)
        {
            var list = new List<string>();

            using (StreamReader reader = new StreamReader(await blockBlob.OpenReadAsync()))
            {
                list.Add(reader.ReadToEnd());
            }

            return list.ToArray();
        }
    }
}