using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public abstract class MongoCsvImporter : IMongoCsvImporter
    {
        private readonly string _path;

        protected MongoCsvImporter(string path = "")
        {
            _path = path;
        }

        public List<TidyData> Data(DataCsvFilename dataCsvFilename,
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate)
        {
            var file = dataCsvFilename + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));

            var path = newPath + "/wwwroot/data/" + file;

            Console.WriteLine("Reading data from:" + path);

            var headers = File.ReadLines(path).First().Split(',').ToList();

            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => TidyDataFromCsv(x, headers, publicationId, releaseId, releaseDate)).ToList();

            return data.ToList();
        }

        protected abstract TidyData TidyDataFromCsv(string csvLine,
            List<string> headers,
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate);
    }
}