using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Seeds;

public record DataSetSeed(string Filename, DataSet DataSet, Guid DataSetVersionId, Guid? ReleaseFileId = null)
{
    private static readonly Guid ExclusionsPublicationId = new("346fd6f2-3938-4006-9867-08dc1c5c66c3");
    private static readonly Guid PupilAbsencePublicationId = new("d40523f4-50ba-4896-9866-08dc1c5c66c3");
    private static readonly Guid PerformanceTestingPublicationId = new("cbbd299f-8297-44bc-92ac-558bcf51f8ad");

    public static DataSetSeed SpcEthnicityLanguage => new(
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
        DataSetVersionId: new Guid("018c7db8-d7bb-77e8-8ee7-147a705c1e3e")
    );

    public static DataSetSeed SpcYearGroupGender => new(
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
        DataSetVersionId: new Guid("018c7db9-3418-7b58-aa76-6a55d0d7b146")
    );

    public static DataSetSeed AbsenceByCharacteristic2016 => new(
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
        ReleaseFileId: new Guid("59a2afde-3949-4204-98c3-0c49e7358fa3")
    );

    public static DataSetSeed AbsenceRatesCharacteristic => new(
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
        DataSetVersionId: new Guid("018c7db9-6a8f-77e1-a789-2eb0f071ef33")
    );

    public static DataSetSeed AbsenceRatesGeographicLevel => new(
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
        DataSetVersionId: new Guid("018c7db9-c6e5-70b6-869f-2b177b8ad324")
    );

    public static DataSetSeed AbsenceRatesGeographicLevelSchool => new(
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
        DataSetVersionId: new Guid("018c7db9-f885-7897-af0a-8f62ac55d0f4")
    );

    public static DataSetSeed ExclusionsByGeographicLevel => new(
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
        ReleaseFileId: new Guid("7d602917-2c16-4745-c086-08dc1c5c7ea7")
    );

    public static DataSetSeed Qua01 => new(
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
        DataSetVersionId: new Guid("018c7dba-1821-747b-9a7c-c91169543a81")
    );

    public static DataSetSeed Nat01 => new(
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
        DataSetVersionId: new Guid("018c7dba-582f-7a22-8e01-67e0a6b43603")
    );
}
