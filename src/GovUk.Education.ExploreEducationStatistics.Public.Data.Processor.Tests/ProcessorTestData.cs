using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public record ProcessorTestData(
    string Name,
    (GeographicLevel Level, List<string> Options)[] LocationsByGeographicLevel,
    (string Label, string[] Options)[] FiltersAndOptions,
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
        LocationsByGeographicLevel:
        [
            (
                Level: GeographicLevel.LocalAuthority,
                Options:
                [
                    "Barnet",
                    "Barnsley",
                    "Kingston upon Thames / Richmond upon Thames",
                    "Sheffield"
                ]),
            (
                Level: GeographicLevel.Country,
                Options:
                [
                    "England"
                ]),
            (
                Level: GeographicLevel.Region,
                Options:
                [
                    "Outer London",
                    "Yorkshire and The Humber"
                ]),
            (
                Level: GeographicLevel.School,
                Options:
                [
                    "Colindale Primary School",
                    "Greenhill Primary School",
                    "Hoyland Springwood Primary School",
                    "King Athelstan Primary School",
                    "Newfield Secondary School",
                    "Penistone Grammar School",
                    "The Kingston Academy",
                    "Wren Academy Finchley"
                ])
        ],
        FiltersAndOptions:
        [
            (
                Label: "Academy type",
                Options:
                [
                    "Primary sponsor led academy",
                    "Secondary free school",
                    "Secondary sponsor led academy"
                ]
            ),
            (
                Label: "National Curriculum year",
                Options:
                [
                    "Year 4",
                    "Year 6",
                    "Year 8",
                    "Year 10"
                ]
            ),
            (
                Label: "School type",
                Options:
                [
                    "State-funded primary",
                    "State-funded secondary",
                    "Total"
                ]
            )
        ],
        Indicators:
        [
            "Enrolments",
            "Number of authorised sessions",
            "Number of possible sessions",
            "Number of unauthorised sessions",
            "Percentage of unauthorised sessions"
        ],
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
