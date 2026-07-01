using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserPreReleaseRoleGeneratorExtensions
{
    public static Generator<UserPreReleaseRole> DefaultUserPreReleaseRole(this DataFixture fixture) =>
        fixture.Generator<UserPreReleaseRole>().WithDefaults();

    public static Generator<UserPreReleaseRole> WithDefaults(this Generator<UserPreReleaseRole> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<UserPreReleaseRole> WithReleaseVersion(
        this Generator<UserPreReleaseRole> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<UserPreReleaseRole> WithReleaseVersionId(
        this Generator<UserPreReleaseRole> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<UserPreReleaseRole> WithReleaseVersions(
        this Generator<UserPreReleaseRole> generator,
        IEnumerable<ReleaseVersion> releaseVersions
    )
    {
        releaseVersions.ForEach(
            (releaseVersion, index) => generator.ForIndex(index, s => s.SetReleaseVersion(releaseVersion))
        );

        return generator;
    }

    public static Generator<UserPreReleaseRole> WithUser(this Generator<UserPreReleaseRole> generator, User user) =>
        generator.ForInstance(s => s.SetUser(user));

    public static Generator<UserPreReleaseRole> WithUserId(this Generator<UserPreReleaseRole> generator, Guid userId) =>
        generator.ForInstance(s => s.SetUserId(userId));

    public static Generator<UserPreReleaseRole> WithCreated(
        this Generator<UserPreReleaseRole> generator,
        DateTimeOffset created
    ) => generator.ForInstance(s => s.SetCreated(created));

    public static Generator<UserPreReleaseRole> WithCreatedBy(
        this Generator<UserPreReleaseRole> generator,
        User createdBy
    ) => generator.ForInstance(s => s.SetCreatedBy(createdBy));

    public static Generator<UserPreReleaseRole> WithCreatedById(
        this Generator<UserPreReleaseRole> generator,
        Guid createdById
    ) => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static Generator<UserPreReleaseRole> WithEmailSent(
        this Generator<UserPreReleaseRole> generator,
        DateTimeOffset emailSent
    ) => generator.ForInstance(s => s.SetEmailSent(emailSent));

    public static InstanceSetters<UserPreReleaseRole> SetDefaults(this InstanceSetters<UserPreReleaseRole> setters) =>
        setters.SetDefault(uprr => uprr.Id).SetDefault(uprr => uprr.ReleaseVersionId).SetDefault(uprr => uprr.UserId);

    public static InstanceSetters<UserPreReleaseRole> SetReleaseVersion(
        this InstanceSetters<UserPreReleaseRole> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(uprr => uprr.ReleaseVersion, releaseVersion).SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<UserPreReleaseRole> SetReleaseVersionId(
        this InstanceSetters<UserPreReleaseRole> setters,
        Guid releaseVersionId
    ) => setters.Set(uprr => uprr.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<UserPreReleaseRole> SetUser(
        this InstanceSetters<UserPreReleaseRole> setters,
        User user
    ) => setters.Set(uprr => uprr.User, user).SetUserId(user.Id);

    public static InstanceSetters<UserPreReleaseRole> SetUserId(
        this InstanceSetters<UserPreReleaseRole> setters,
        Guid userId
    ) => setters.Set(uprr => uprr.UserId, userId);

    public static InstanceSetters<UserPreReleaseRole> SetCreated(
        this InstanceSetters<UserPreReleaseRole> setters,
        DateTimeOffset created
    ) => setters.Set(uprr => uprr.Created, created);

    public static InstanceSetters<UserPreReleaseRole> SetCreatedBy(
        this InstanceSetters<UserPreReleaseRole> setters,
        User createdBy
    ) => setters.Set(uprr => uprr.CreatedBy, createdBy).SetCreatedById(createdBy.Id);

    public static InstanceSetters<UserPreReleaseRole> SetCreatedById(
        this InstanceSetters<UserPreReleaseRole> setters,
        Guid createdById
    ) => setters.Set(uprr => uprr.CreatedById, createdById);

    public static InstanceSetters<UserPreReleaseRole> SetEmailSent(
        this InstanceSetters<UserPreReleaseRole> setters,
        DateTimeOffset emailSent
    ) => setters.Set(uprr => uprr.EmailSent, emailSent);
}
