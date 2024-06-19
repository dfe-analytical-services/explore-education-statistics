using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public record ProcessorTestData(
    string Name,
    GeographicLevel[] GeographicLevels,
    string[] Filters,
    string[] Indicators,
    TimePeriodRange TimePeriodRange)
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

    public static ProcessorTestData AbsenceSchool => new(
        Name: nameof(AbsenceSchool),
        GeographicLevels: [GeographicLevel.LocalAuthority, GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.School],
        Filters: ["Academy type", "National Curriculum year", "School type"],
        Indicators: ["Enrolments", "Number of authorised sessions", "Number of possible sessions", "Number of unauthorised sessions", "Percentage of unauthorised sessions"],
        TimePeriodRange: new TimePeriodRange
        {
            Start = new TimePeriodRangeBound
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2020/2021"
            },
            End = new TimePeriodRangeBound
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2022/2023"
            }
        });
}
