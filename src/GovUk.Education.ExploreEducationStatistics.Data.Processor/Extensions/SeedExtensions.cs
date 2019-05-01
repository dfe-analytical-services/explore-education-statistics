namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions
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
            using (StreamReader sr = new StreamReader(await blockBlob.OpenReadAsync()))
            {
                while (sr.Peek() >= 0)
                {
                    list.Add(sr.ReadLine());
                }
            }

            return list.ToArray();
        }
    }
}