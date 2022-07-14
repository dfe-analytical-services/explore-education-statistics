#nullable enable
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStorageUtils
    {
        public static string GetExtension(string fileName)
        {
            return Path.GetExtension(fileName)?.TrimStart('.') ?? string.Empty;
        }

        public static int CalculateNumberOfRows(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                var numberOfLines = 0;
                while (reader.ReadLine() != null)
                {
                    ++numberOfLines;
                }

                return numberOfLines;
            }
        }
    }
}
