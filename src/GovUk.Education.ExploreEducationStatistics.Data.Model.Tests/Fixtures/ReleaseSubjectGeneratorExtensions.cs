#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseSubjectGeneratorExtensions
{
    public static Generator<ReleaseSubject> DefaultReleaseSubject(this DataFixture fixture) =>
        fixture.Generator<ReleaseSubject>().WithDefaults();

    public static Generator<ReleaseSubject> WithDefaults(this Generator<ReleaseSubject> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<ReleaseSubject> WithReleaseVersion(
        this Generator<ReleaseSubject> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<ReleaseSubject> WithReleaseVersionId(
        this Generator<ReleaseSubject> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<ReleaseSubject> WithSubject(this Generator<ReleaseSubject> generator, Subject subject) =>
        generator.ForInstance(s => s.SetSubject(subject));

    public static Generator<ReleaseSubject> WithSubjectId(this Generator<ReleaseSubject> generator, Guid subjectId) =>
        generator.ForInstance(s => s.SetSubjectId(subjectId));

    public static Generator<ReleaseSubject> WithReleaseVersions(
        this Generator<ReleaseSubject> generator,
        IEnumerable<ReleaseVersion> releaseVersions
    )
    {
        releaseVersions.ForEach(
            (releaseVersion, index) => generator.ForIndex(index, s => s.SetReleaseVersion(releaseVersion))
        );

        return generator;
    }

    public static Generator<ReleaseSubject> WithSubjects(
        this Generator<ReleaseSubject> generator,
        IEnumerable<Subject> subjects
    )
    {
        subjects.ForEach((subject, index) => generator.ForIndex(index, s => s.SetSubject(subject)));

        return generator;
    }

    public static InstanceSetters<ReleaseSubject> SetDefaults(this InstanceSetters<ReleaseSubject> setters) =>
        setters.SetDefault(rs => rs.ReleaseVersionId).SetDefault(rs => rs.SubjectId);

    public static InstanceSetters<ReleaseSubject> SetReleaseVersion(
        this InstanceSetters<ReleaseSubject> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(rs => rs.ReleaseVersion, releaseVersion).SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<ReleaseSubject> SetReleaseVersionId(
        this InstanceSetters<ReleaseSubject> setters,
        Guid releaseVersionId
    ) => setters.Set(rs => rs.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<ReleaseSubject> SetSubject(
        this InstanceSetters<ReleaseSubject> setters,
        Subject subject
    ) => setters.Set(rs => rs.Subject, subject).SetSubjectId(subject.Id);

    public static InstanceSetters<ReleaseSubject> SetSubjectId(
        this InstanceSetters<ReleaseSubject> setters,
        Guid subjectId
    ) => setters.Set(rs => rs.SubjectId, subjectId);
}
