using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions
{
    public static class FileExtensions
    {
        public static IEnumerable<string> GetCsvLines(this SubjectData subjectData)
        {
            return ReadAllLinesAsync(subjectData.DataBlob).Result;
        }

        public static IEnumerable<string> GetMetaLines(this SubjectData subjectData)
        {
            return ReadAllLinesAsync(subjectData.MetaBlob).Result;
        }

        private static async Task<IEnumerable<string>> ReadAllLinesAsync(CloudBlob blockBlob)
        {
            var list = new List<string>();
            using (var sr = new StreamReader(await blockBlob.OpenReadAsync()))
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