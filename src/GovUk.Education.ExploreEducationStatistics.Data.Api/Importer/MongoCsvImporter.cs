using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public abstract class MongoCsvImporter<T> where T : TidyData
    {
        private readonly string _path;

        protected MongoCsvImporter(string path = "")
        {
            _path = path;
        }

        public List<T> Data(DataCsvFilename dataCsvFilename)
        {
            var file = dataCsvFilename + ".csv";
            var directory = Directory.GetCurrentDirectory();
            var newPath = Path.GetFullPath(Path.Combine(directory, _path));

            var path = newPath + "/wwwroot/data/" + file;

            Console.WriteLine("Reading data from:" + path);

            var headers = File.ReadLines(path).First().Split(',').ToList();

            var data = File.ReadAllLines(path)
                .Skip(1)
                .Select(x => TidyDataFromCsv(x, headers)).ToList();

            Console.WriteLine(data.Count + " rows");
            return data.ToList();
        }

        protected abstract T TidyDataFromCsv(string csvLine, List<string> headers);
    }
}