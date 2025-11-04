using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Seeds;

public record DataSetSeed(string Filename, DataSet DataSet, Guid DataSetVersionId, Release Release)
{
    private static readonly Guid ExclusionsPublicationId = new("346fd6f2-3938-4006-9867-08dc1c5c66c3");
    private static readonly Guid PupilAbsencePublicationId = new("d40523f4-50ba-4896-9866-08dc1c5c66c3");
    private static readonly Guid PerformanceTestingPublicationId = new("cbbd299f-8297-44bc-92ac-558bcf51f8ad");

    public static DataSetSeed SpcEthnicityLanguage =>
        new(
            Filename: nameof(SpcEthnicityLanguage),
            DataSet: new DataSet
            {
                Id = new Guid("018c68f3-51af-70ff-b6b5-719cff3d869e"),
                Title = "Pupil characteristics - Ethnicity and Language",
                Summary =
                    "Number of pupils in state-funded nursery, primary, secondary and special schools, non-maintained special schools and pupil referral units by language and ethnicity.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-06-15T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-06-01T12:00:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-06-15T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7db8-d7bb-77e8-8ee7-147a705c1e3e"),
            Release: new Release
            {
                DataSetFileId = new Guid("bca47ddb-c46f-4608-8d94-fae70c627b6c"),
                ReleaseFileId = new Guid("5ec29da5-f485-41b5-837f-37e81435b38a"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed SpcYearGroupGender =>
        new(
            Filename: nameof(SpcYearGroupGender),
            DataSet: new DataSet
            {
                Id = new Guid("018c68f3-7aa0-7e1e-895e-6a9587ee31d0"),
                Title = "Pupil characteristics - Year group and Gender",
                Summary =
                    "Number of pupils in state-funded nursery, primary, secondary and special schools, non-maintained special schools, pupil referral units and independent schools by national curriculum year and gender.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-06-16T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-06-02T12:00:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-06-16T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7db9-3418-7b58-aa76-6a55d0d7b146"),
            Release: new Release
            {
                DataSetFileId = new Guid("4499155a-5858-4384-8d87-424b8abf53d0"),
                ReleaseFileId = new Guid("a3af86f4-5911-44cb-b45b-fd70f5422308"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed AbsenceByCharacteristic2016 =>
        new(
            Filename: nameof(AbsenceByCharacteristic2016),
            DataSet: new DataSet
            {
                Id = new Guid("018f6304-42aa-7731-af09-acfa105c6fca"),
                Title = "Absence by characteristic",
                Summary = "Absence by characteristic data guidance content",
                Status = DataSetStatus.Published,
                PublicationId = PupilAbsencePublicationId,
                Published = DateTimeOffset.Parse("2024-01-25T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2024-01-24T12:00+00:00"),
                Updated = DateTimeOffset.Parse("2024-01-24T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018f6306-cb44-7fcd-80cc-bfdb9e1ce5a1"),
            Release: new Release
            {
                DataSetFileId = new Guid("2679370e-6432-43ff-813e-5b64af5283ab"),
                ReleaseFileId = new Guid("59a2afde-3949-4204-98c3-0c49e7358fa3"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed AbsenceRatesCharacteristic =>
        new(
            Filename: nameof(AbsenceRatesCharacteristic),
            DataSet: new DataSet
            {
                Id = new Guid("018c696b-9ebb-7a62-ae29-2dc3ff52b02a"),
                Title = "Absence rates by pupil characteristic - full academic years",
                Summary =
                    "Absence information for the full academic year, by pupil characteristics including SEN, FSM, language, year group, gender and ethnicity for England.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-09-01T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-08-15T12:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-09-01T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7db9-6a8f-77e1-a789-2eb0f071ef33"),
            Release: new Release
            {
                DataSetFileId = new Guid("814dd82a-6e4e-4517-b37c-6ced612add78"),
                ReleaseFileId = new Guid("2e60f344-06a1-4c33-b325-0301c7f840b5"),
                Slug = "2020-21",
                Title = "Academic year 2020/21",
            }
        );

    public static DataSetSeed AbsenceRatesGeographicLevel =>
        new(
            Filename: nameof(AbsenceRatesGeographicLevel),
            DataSet: new DataSet
            {
                Id = new Guid("018c696b-b93e-7f43-8b93-88de6cace1cd"),
                Title = "Absence rates by geographic level - full academic years",
                Summary =
                    "Absence information for full academic years for all enrolments in state-funded primary, secondary and special schools including information on overall absence, persistent absence and reason for absence for pupils aged 5-15.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-09-02T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-08-16T12:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-09-02T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7db9-c6e5-70b6-869f-2b177b8ad324"),
            Release: new Release
            {
                DataSetFileId = new Guid("81684e51-c713-49e6-a5c5-d020b1357ad1"),
                ReleaseFileId = new Guid("9a601953-30ef-4fcd-b079-8428057d3e5d"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed AbsenceRatesGeographicLevelSchool =>
        new(
            Filename: nameof(AbsenceRatesGeographicLevelSchool),
            DataSet: new DataSet
            {
                Id = new Guid("018c696b-c8ea-7376-ba7b-9817339f12d8"),
                Title = "Absence rates by geographic level - school level - full academic years",
                Summary =
                    "Absence information for full academic years for all enrolments in state-funded primary, secondary and special schools including information on overall absence, persistent absence and reason for absence for pupils aged 5-15. Includes school level data.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-09-03T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-08-17T12:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-09-03T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7db9-f885-7897-af0a-8f62ac55d0f4"),
            Release: new Release
            {
                DataSetFileId = new Guid("222f37ca-adf3-40a7-9404-53332880cb7f"),
                ReleaseFileId = new Guid("c105a86d-e1ed-4dde-8156-cff3f8637e9e"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed ExclusionsByGeographicLevel =>
        new(
            Filename: nameof(ExclusionsByGeographicLevel),
            DataSet: new DataSet
            {
                Id = new Guid("018f630d-f1f7-7268-8af3-7e8eba892947"),
                Title = "Exclusions by geographic level",
                Summary = "Exclusions by geographic level data guidance content",
                Status = DataSetStatus.Published,
                PublicationId = ExclusionsPublicationId,
                Published = DateTimeOffset.Parse("2024-01-25T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2024-01-24T12:00+00:00"),
                Updated = DateTimeOffset.Parse("2024-01-24T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018f630e-bd6c-7ea5-94e6-d12c361ef2b8"),
            Release: new Release
            {
                DataSetFileId = new Guid("3db90234-2b21-4181-8e29-4f9d61c53823"),
                ReleaseFileId = new Guid("7d602917-2c16-4745-c086-08dc1c5c7ea7"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed Qua01 =>
        new(
            Filename: nameof(Qua01),
            DataSet: new DataSet
            {
                Id = new Guid("018c696c-ac3d-74f1-88b6-b6e78d4fc18b"),
                Title = "Destinations by qualification title, provision and sector subject area (QUA01)",
                Summary =
                    "Reports on the employment, and learning destinations of adult FE & Skills learners, all age apprentices that achieved their learning aim, and Traineeship learners that completed their aim. Destination rates are calculated as a proportion of learners for whom a match was found in the LEO data.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-12-01T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-11-01T09:30:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-12-01T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7dba-1821-747b-9a7c-c91169543a81"),
            Release: new Release
            {
                DataSetFileId = new Guid("d4abb520-a1b4-4d8e-873c-58164794f387"),
                ReleaseFileId = new Guid("74983e80-fccd-4e52-8b6f-fb50f4e58b24"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );

    public static DataSetSeed Nat01 =>
        new(
            Filename: nameof(Nat01),
            DataSet: new DataSet
            {
                Id = new Guid("018c696d-333b-775a-a943-29082f7fe894"),
                Title = "Destinations by demographics and provision (NAT01)",
                Summary =
                    "Reports on the employment and learning destinations of adult FE & skills learners, all age apprentices that achieved their learning aim, and traineeship learners that completed their aim. Destination rates are calculated as a proportion of learners for whom a match was found in the LEO data.",
                Status = DataSetStatus.Published,
                PublicationId = PerformanceTestingPublicationId,
                Published = DateTimeOffset.Parse("2023-12-02T09:30:00+00:00"),
                Created = DateTimeOffset.Parse("2023-11-02T09:30:00+00:00"),
                Updated = DateTimeOffset.Parse("2023-12-02T09:30:00+00:00"),
            },
            DataSetVersionId: new Guid("018c7dba-582f-7a22-8e01-67e0a6b43603"),
            Release: new Release
            {
                DataSetFileId = new Guid("685d451d-b024-4002-9913-80f35856379e"),
                ReleaseFileId = new Guid("297a5379-6c1f-4f99-bc29-f1e9121c98bc"),
                Slug = "2016-17",
                Title = "Academic year 2016/17",
            }
        );
}
