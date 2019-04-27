namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using System.Collections.Generic;
    using System.IO;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

    public static class SeedExtensions
    {
        public static IEnumerable<string> GetCsvLines(this Subject subject)
        {
            return ReadAllLines(subject.Filename.ToString());
        }

        public static IEnumerable<string> GetMetaLines(this Subject subject)
        {
            return ReadAllLines(subject.GetMetaFilename().ToString());
        }

        private static IEnumerable<string> ReadAllLines(string filename)
        {
            var file = filename + ".csv";
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Files/" + file));
            return File.ReadAllLines(path);
        }
    }
}