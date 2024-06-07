using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public record ProcessorTestData(string Name)
{
    private static string DataFilesDirectoryPath => Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "DataFiles"
    );

    private string DirectoryPath => Path.Combine(DataFilesDirectoryPath, Name);

    public string CsvDataFilePath => Path.Combine(DirectoryPath, "data.csv");
    public string CsvDataGzipFilePath => Path.Combine(DirectoryPath, "data.csv.gz");
    public string CsvMetadataFilePath => Path.Combine(DirectoryPath, "metadata.csv");

    public static ProcessorTestData AbsenceSchool => new(nameof(AbsenceSchool));
}
