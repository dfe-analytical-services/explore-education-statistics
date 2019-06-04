using System.Collections.Generic;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions
{
    public static class SeedExtensions
    {
        public static IEnumerable<string> GetCsvLines(this DataCsvFile file)
        {
            return ReadAllLines(file.ToString());
        }

        public static IEnumerable<string> GetMetaCsvLines(this DataCsvFile file)
        {
            return ReadAllLines(file + ".meta");
        }

        private static IEnumerable<string> ReadAllLines(string filename)
        {
            var file = filename + ".csv";
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Files/" + file));
            return File.ReadAllLines(path);
        }
    }
}