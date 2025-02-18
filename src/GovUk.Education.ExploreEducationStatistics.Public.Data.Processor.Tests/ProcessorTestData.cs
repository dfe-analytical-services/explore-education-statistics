using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

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
                Id = 1,
                Level = GeographicLevel.LocalAuthority,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 1,
                        OldCode = "370",
                        Code = "E08000016",
                        Label = "Barnsley",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 2,
                        OldCode = "373",
                        Code = "E08000019",
                        Label = "Sheffield",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 3,
                        OldCode = "302",
                        Code = "E09000003",
                        Label = "Barnet",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 4,
                        OldCode = "314",
                        Code = "E09000021 / E09000027",
                        Label = "Kingston upon Thames / Richmond upon Thames",
                    },
                ]
            },
            new LocationMeta
            {
                Id = 2,
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
                Id = 3,
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
                Id = 4,
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
                Id = 1,
                PublicId = SqidEncoder.Encode(1),
                Column = "academy_type",
                Label = "Academy type",
                Hint = "Only applicable for academies, otherwise no value",
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 1,
                        Label = "Primary sponsor led academy",
                    },
                    new FilterOptionMeta
                    {
                        Id = 2,
                        Label = "Secondary free school",
                    },
                    new FilterOptionMeta
                    {
                        Id = 3,
                        Label = "Secondary sponsor led academy",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 1,
                        OptionId = 1,
                        PublicId = SqidEncoder.Encode(1),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 1,
                        OptionId = 2,
                        PublicId = SqidEncoder.Encode(2),
                    },
                    new FilterOptionMetaLink
                    {

                        MetaId = 1,
                        OptionId = 3,
                        PublicId = SqidEncoder.Encode(3),
                    }
                ],
            },
            new FilterMeta
            {
                Id = 2,
                PublicId = SqidEncoder.Encode(2),
                Column = "ncyear",
                Label = "National Curriculum year",
                Hint = "Ranges from years 1 to 11",
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 4,
                        Label = "Year 10",
                    },
                    new FilterOptionMeta
                    {
                        Id = 5,
                        Label = "Year 4",
                    },
                    new FilterOptionMeta
                    {
                        Id = 6,
                        Label = "Year 6",
                    },
                    new FilterOptionMeta
                    {
                        Id = 7,
                        Label = "Year 8",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 4,
                        PublicId = SqidEncoder.Encode(4),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 5,
                        PublicId = SqidEncoder.Encode(5),
                    },
                    new FilterOptionMetaLink
                    {

                        MetaId = 2,
                        OptionId = 6,
                        PublicId = SqidEncoder.Encode(6),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 7,
                        PublicId = SqidEncoder.Encode(7),
                    }
                ],
            },
            new FilterMeta
            {
                Id = 3,
                PublicId = SqidEncoder.Encode(3),
                Column = "school_type",
                Label = "School type",
                Hint = "",
                DefaultOptionId = 10,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 8,
                        Label = "State-funded primary",
                    },
                    new FilterOptionMeta
                    {
                        Id = 9,
                        Label = "State-funded secondary",
                    },
                    new FilterOptionMeta
                    {
                        Id = 10,
                        Label = "Total",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 8,
                        PublicId = SqidEncoder.Encode(8),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 9,
                        PublicId = SqidEncoder.Encode(9),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 10,
                        PublicId = SqidEncoder.Encode(10),
                    }
                ]
            },
        ],
        ExpectedIndicators =
        [
            new IndicatorMeta
            {
                Id = 1,
                PublicId = SqidEncoder.Encode(1),
                Column = "enrolments",
                Label = "Enrolments",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                Id = 2,
                PublicId = SqidEncoder.Encode(2),
                Column = "sess_authorised",
                Label = "Number of authorised sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                Id = 3,
                PublicId = SqidEncoder.Encode(3),
                Column = "sess_possible",
                Label = "Number of possible sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                Id = 4,
                PublicId = SqidEncoder.Encode(4),
                Column = "sess_unauthorised",
                Label = "Number of unauthorised sessions",
                DecimalPlaces = 0,
                DataSetVersionId = Guid.Empty,
            },
            new IndicatorMeta
            {
                Id = 5,
                PublicId = SqidEncoder.Encode(5),
                Column = "sess_unauthorised_percent",
                Label = "Percentage of unauthorised sessions",
                DecimalPlaces = 2,
                Unit = IndicatorUnit.Percent,
                DataSetVersionId = Guid.Empty
            },
        ],
    };

    public static ProcessorTestData FilterDefaultOptions => new()
    {
        Name = nameof(FilterDefaultOptions),
        ExpectedTotalResults = 28,
        ExpectedTimePeriods =
        [
            new TimePeriodMeta
            {
                Code = TimeIdentifier.CalendarYear,
                Period = "2018",
                DataSetVersionId = Guid.Empty,
            },
        ],
        ExpectedGeographicLevels =
        [
            GeographicLevel.LocalAuthority,
        ],
        ExpectedLocations =
        [
            new LocationMeta
            {
                Id = 1,
                Level = GeographicLevel.LocalAuthority,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 1,
                        OldCode = "370",
                        Code = "E08000016",
                        Label = "Barnsley",
                    },
                    new LocationLocalAuthorityOptionMeta
                    {
                        Id = 2,
                        OldCode = "330",
                        Code = "E08000025",
                        Label = "Birmingham",
                    },
                ]
            },
            new LocationMeta
            {
                Id = 2,
                Level = GeographicLevel.Country,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new LocationCodedOptionMeta
                    {
                        Id = 3,
                        Code = "E92000001",
                        Label = "England",
                    },
                ]
            },
        ],
        ExpectedFilters =
        [
            new FilterMeta
            {
                Id = 1,
                PublicId = SqidEncoder.Encode(1),
                Column = "qualification_level",
                Label = "Level of qualification",
                DataSetVersionId = Guid.Empty,
                DefaultOptionId = 3,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 3,
                        Label = "All",
                    },
                    new FilterOptionMeta
                    {
                        Id = 1,
                        Label = "Entry level",
                    },
                    new FilterOptionMeta
                    {
                        Id = 2,
                        Label = "Higher",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 1,
                        OptionId = 1,
                        PublicId = SqidEncoder.Encode(1),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 1,
                        OptionId = 2,
                        PublicId = SqidEncoder.Encode(2),
                    },
                    new FilterOptionMetaLink
                    {

                        MetaId = 1,
                        OptionId = 3,
                        PublicId = SqidEncoder.Encode(3),
                    },
                ],
            },
            new FilterMeta
            {
                Id = 2,
                PublicId = SqidEncoder.Encode(2),
                Column = "course_title",
                Label = "Name of course being studied",
                DefaultOptionId = 10,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 11,
                        Label = "Biochemistry",
                    },
                    new FilterOptionMeta
                    {
                        Id = 6,
                        Label = "Bricklaying",
                    },
                    new FilterOptionMeta
                    {
                        Id = 4,
                        Label = "Civil engineering",
                    },
                    new FilterOptionMeta
                    {
                        Id = 12,
                        Label = "Electrical engineering",
                    },
                    new FilterOptionMeta
                    {
                        Id = 9,
                        Label = "Forestry",
                    },
                    new FilterOptionMeta
                    {
                        Id = 8,
                        Label = "Hedge rows",
                    },
                    new FilterOptionMeta
                    {
                        Id = 5,
                        Label = "Physics",
                    },
                    new FilterOptionMeta
                    {
                        Id = 7,
                        Label = "Plastering",
                    },
                    new FilterOptionMeta
                    {
                        Id = 10,
                        Label = "Total",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 11,
                        PublicId = SqidEncoder.Encode(4),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 6,
                        PublicId = SqidEncoder.Encode(5),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 4,
                        PublicId = SqidEncoder.Encode(6),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 12,
                        PublicId = SqidEncoder.Encode(7),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 9,
                        PublicId = SqidEncoder.Encode(8),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 8,
                        PublicId = SqidEncoder.Encode(9),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 5,
                        PublicId = SqidEncoder.Encode(10),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 7,
                        PublicId = SqidEncoder.Encode(11),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 2,
                        OptionId = 10,
                        PublicId = SqidEncoder.Encode(12),
                    }
                ]
            },
            new FilterMeta
            {
                Id = 3,
                PublicId = SqidEncoder.Encode(3),
                Column = "subject_area",
                Label = "Sector subject area",
                DefaultOptionId = 17,
                DataSetVersionId = Guid.Empty,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 17,
                        Label = "All areas",
                    },
                    new FilterOptionMeta
                    {
                        Id = 13,
                        Label = "Construction",
                    },
                    new FilterOptionMeta
                    {
                        Id = 16,
                        Label = "Engineering",
                    },
                    new FilterOptionMeta
                    {
                        Id = 14,
                        Label = "Horticulture",
                    },
                    new FilterOptionMeta
                    {
                        Id = 15,
                        Label = "Science",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 17,
                        PublicId = SqidEncoder.Encode(13),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 13,
                        PublicId = SqidEncoder.Encode(14),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 16,
                        PublicId = SqidEncoder.Encode(15),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 14,
                        PublicId = SqidEncoder.Encode(16),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 3,
                        OptionId = 15,
                        PublicId = SqidEncoder.Encode(17),
                    }
                ]
            },
            new FilterMeta
            {
                Id = 4,
                PublicId = SqidEncoder.Encode(4),
                Column = "qualification_type",
                Label = "Type of qualification",
                DataSetVersionId = Guid.Empty,
                DefaultOptionId = 10,
                Options =
                [
                    new FilterOptionMeta
                    {
                        Id = 18,
                        Label = "Academic",
                    },
                    new FilterOptionMeta
                    {
                        Id = 10,
                        Label = "Total",
                    },
                    new FilterOptionMeta
                    {
                        Id = 19,
                        Label = "Vocational",
                    },
                ],
                OptionLinks =
                [
                    new FilterOptionMetaLink
                    {
                        MetaId = 4,
                        OptionId = 18,
                        PublicId = SqidEncoder.Encode(18),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 4,
                        OptionId = 10,
                        PublicId = SqidEncoder.Encode(19),
                    },
                    new FilterOptionMetaLink
                    {
                        MetaId = 4,
                        OptionId = 19,
                        PublicId = SqidEncoder.Encode(20),
                    }
                ],
            },
        ],
        ExpectedIndicators =
        [
            new IndicatorMeta
            {
                Id = 1,
                PublicId = SqidEncoder.Encode(1),
                Column = "enrollment_count",
                Label = "Number of students enrolled",
                DecimalPlaces = 0,
                Unit = IndicatorUnit.None,
                DataSetVersionId = Guid.Empty,
            },
        ],
    };

    private static string DataFilesDirectoryPath => Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "DataFiles"
    );
}
