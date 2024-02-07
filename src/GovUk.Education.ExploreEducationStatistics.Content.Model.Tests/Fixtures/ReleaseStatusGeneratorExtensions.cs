using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseStatusGeneratorExtensions
{
    public static Generator<ReleaseStatus> DefaultReleaseStatus(this DataFixture fixture)
        => fixture.Generator<ReleaseStatus>().WithDefaults();

    public static Generator<ReleaseStatus> WithDefaults(this Generator<ReleaseStatus> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<ReleaseStatus> SetDefaults(this InstanceSetters<ReleaseStatus> setters)
        => setters
            .SetDefault(rs => rs.Id)
            .SetDefault(rs => rs.ApprovalStatus)
            .SetDefault(rs => rs.InternalReleaseNote)
            .Set(rs => rs.Created, f => f.Date.Past())
            .SetDefault(rs => rs.CreatedById);

    public static Generator<ReleaseStatus> WithRelease(
        this Generator<ReleaseStatus> generator,
        Release release)
        => generator.ForInstance(s => s.SetRelease(release));

    public static Generator<ReleaseStatus> WithApprovalStatus(
        this Generator<ReleaseStatus> generator,
        ReleaseApprovalStatus approvalStatus)
        => generator.ForInstance(s => s.SetApprovalStatus(approvalStatus));

    public static Generator<ReleaseStatus> WithInternalReleaseNote(
        this Generator<ReleaseStatus> generator,
        string? internalReleaseNote = null)
        => generator.ForInstance(s => s.SetInternalReleaseNote(internalReleaseNote));

    public static Generator<ReleaseStatus> WithCreated(
        this Generator<ReleaseStatus> generator,
        DateTime? created = null)
    {
        return generator.ForInstance(s => s.SetCreated(created));
    }

    public static Generator<ReleaseStatus> WithCreatedBy(
        this Generator<ReleaseStatus> generator,
        User? createdBy = null)
    {
        return generator.ForInstance(s => s.SetCreatedBy(createdBy));
    }

    public static InstanceSetters<ReleaseStatus> SetRelease(
        this InstanceSetters<ReleaseStatus> setters,
        Release release)
        => setters.Set(rs => rs.Release, release)
            .SetReleaseId(release.Id);

    public static InstanceSetters<ReleaseStatus> SetReleaseId(
        this InstanceSetters<ReleaseStatus> setters,
        Guid releaseId)
        => setters.Set(rs => rs.ReleaseId, releaseId);

    public static InstanceSetters<ReleaseStatus> SetApprovalStatus(
        this InstanceSetters<ReleaseStatus> setters,
        ReleaseApprovalStatus approvalStatus)
        => setters.Set(rs => rs.ApprovalStatus, approvalStatus);

    public static InstanceSetters<ReleaseStatus> SetInternalReleaseNote(
        this InstanceSetters<ReleaseStatus> setters,
        string? internalReleaseNote)
        => setters.Set(rs => rs.InternalReleaseNote, internalReleaseNote);

    public static InstanceSetters<ReleaseStatus> SetCreated(
        this InstanceSetters<ReleaseStatus> setters,
        DateTime? created)
        => setters.Set(rs => rs.Created, created);

    public static InstanceSetters<ReleaseStatus> SetCreatedBy(
        this InstanceSetters<ReleaseStatus> setters,
        User? createdBy)
        => setters.Set(rs => rs.CreatedBy, createdBy)
            .SetCreatedById(createdBy?.Id);

    public static InstanceSetters<ReleaseStatus> SetCreatedById(
        this InstanceSetters<ReleaseStatus> setters,
        Guid? createdById)
        => setters.Set(rs => rs.CreatedById, createdById);
}
