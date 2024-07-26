using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests;

public record ProcessorTestData
{
    public required string Name { get; init; }

    public required int ExpectedTotalResults { get; init; }

    public required List<GeographicLevel> ExpectedGeographicLevels;

    public required List<FilterMeta> ExpectedFilters { get; init; }

    public required List<IndicatorMeta> ExpectedIndicators { get; init; }

    public required List<LocationMeta> ExpectedLocations { get; init; }

    public required List<TimePeriodMeta> ExpectedTimePeriods { get; init; }

    public string CsvDataFilePath => Path.Combine(DirectoryPath, "data.csv");

    public string CsvDataGzipFilePath => Path.Combine(DirectoryPath, "data.csv.gz");

    public string CsvMetadataFilePath => Path.Combine(DirectoryPath, "metadata.csv");

    private string DirectoryPath => Path.Combine(DataFilesDirectoryPath, Name);

    private static string DataFilesDirectoryPath => Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "DataFiles"
    );

    public static ProcessorTestData AbsenceSchool => new()
    {
        Name = nameof(AbsenceSchool),
        ExpectedTotalResults = 216,
        ExpectedTimePeriods =
        [
            new TimePeriodMeta
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2020/2021",
                DataSetVersionId = Guid.Empty,
            },
            new TimePeriodMeta
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2021/2022",
                DataSetVersionId = Guid.Empty,
            },
            new TimePeriodMeta
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2022/2023",
                DataSetVersionId = Guid.Empty,
            }
        ],
        ExpectedGeographicLevels =
        [
            GeographicLevel.LocalAuthority,
            GeographicLevel.Country,
            GeographicLevel.Region,
            GeographicLevel.School,
        ],
        ExpectedLocations =
        [
            new LocationMeta
            {
                Level = GeographicLevel.LocalAuthority,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 1,
                        OldCode = "302",
                        Code = "E09000003",
                        Label = "Barnet",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 2,
                        OldCode = "314",
                        Code = "E09000021 / E09000027",
                        Label = "Kingston upon Thames / Richmond upon Thames",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 3,
                        OldCode = "370",
                        Code = "E08000016",
                        Label = "Barnsley",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 4,
                        OldCode = "373",
                        Code = "E08000019",
                        Label = "Sheffield",
                    },
                ]
            },
            new LocationMeta
            {
                Level = GeographicLevel.Country,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationCodedOptionMeta
                    {
                        Id = 5,
                        Code = "E92000001",
                        Label = "England",
                    },
                ]
            },
            new LocationMeta
            {
                Level = GeographicLevel.Region,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationCodedOptionMeta
                    {
                        Id = 6,
                        Code = "E12000003",
                        Label = "Yorkshire and The Humber",
                    },
                    new LocationCodedOptionMeta
                    {
                        Id = 7,
                        Code = "E13000002",
                        Label = "Outer London",
                    },
                ]
            },
            new LocationMeta
            {
                Level = GeographicLevel.School,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationSchoolOptionMeta
                    {
                        Id = 8,
                        Urn = "101269",
                        LaEstab = "3022014",
                        Label = "Colindale Primary School",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 9,
                        Urn = "102579",
                        LaEstab = "3142032",
                        Label = "King Athelstan Primary School",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 10,
                        Urn = "106653",
                        LaEstab = "3704027",
                        Label = "Penistone Grammar School",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 11,
                        Urn = "135507",
                        LaEstab = "3026906",
                        Label = "Wren Academy Finchley",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 12,
                        Urn = "140821",
                        LaEstab = "3734008",
                        Label = "Newfield Secondary School",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 13,
                        Urn = "141862",
                        LaEstab = "3144001",
                        Label = "The Kingston Academy",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 14,
                        Urn = "141973",
                        LaEstab = "3702039",
                        Label = "Hoyland Springwood Primary School",
                    },
                    new LocationSchoolOptionMeta
                    {
                        Id = 15,
                        Urn = "145374",
                        LaEstab = "3732341",
                        Label = "Greenhill Primary School",
                    },
                ]
            },
        ],
        ExpectedFilters =
        [
            new FilterMeta
            {
                PublicId = "academy_type",
                Label = "Academy type",
                Hint = "Only applicable for academies, otherwise no value",
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Label = "Primary sponsor led academy",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Secondary free school",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Secondary sponsor led academy",
                    },
                ],
            },
            new FilterMeta
            {
                PublicId = "ncyear",
                Label = "National Curriculum year",
                Hint = "Ranges from years 1 to 11",
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Label = "Year 10",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Year 4",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Year 6",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Year 8",
                    },
                ],
            },
            new FilterMeta
            {
                PublicId = "school_type",
                Label = "School type",
                Hint = "",
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Label = "State-funded primary",
                    },
                    new FilterOptionMeta
                    {
                        Label = "State-funded secondary",
                    },
                    new FilterOptionMeta
                    {
                        Label = "Total",
                        IsAggregate = true
                    },
                ],
            },
        ],
        ExpectedIndicators =
        [
            new IndicatorMeta
            {
                PublicId = "enrolments",
                Label = "Enrolments",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                PublicId = "sess_authorised",
                Label = "Number of authorised sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                PublicId = "sess_possible",
                Label = "Number of possible sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                PublicId = "sess_unauthorised",
                Label = "Number of unauthorised sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                PublicId = "sess_unauthorised_percent",
                Label = "Percentage of unauthorised sessions",
                DecimalPlaces = 2,
                DataSetVersionId = Guid.Empty
            },
        ],
    };
}
