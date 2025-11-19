using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UpdateGeneratorExtensions
{
    public static Generator<Update> DefaultUpdate(this DataFixture fixture) =>
        fixture.Generator<Update>().WithDefaults();

    public static Generator<Update> WithDefaults(this Generator<Update> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Update> SetDefaults(this InstanceSetters<Update> setters) =>
        setters
            .SetDefault(u => u.Id)
            .SetDefault(u => u.On)
            .SetDefault(u => u.Reason)
            .SetDefault(u => u.ReleaseVersionId)
            .Set(u => u.Created, f => f.Date.Past())
            .SetDefault(u => u.CreatedById);

    public static Generator<Update> WithId(this Generator<Update> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<Update> WithOn(this Generator<Update> generator, DateTime on) =>
        generator.ForInstance(s => s.SetOn(on));

    public static Generator<Update> WithReason(this Generator<Update> generator, string reason) =>
        generator.ForInstance(s => s.SetReason(reason));

    public static Generator<Update> WithReleaseVersion(
        this Generator<Update> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<Update> WithReleaseVersionId(this Generator<Update> generator, Guid releaseVersionId) =>
        generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<Update> WithCreated(this Generator<Update> generator, DateTime created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<Update> WithCreatedById(this Generator<Update> generator, Guid createdById) =>
        generator.ForInstance(s => s.SetCreatedById(createdById));

    public static InstanceSetters<Update> SetId(this InstanceSetters<Update> setters, Guid id) =>
        setters.Set(l => l.Id, id);

    public static InstanceSetters<Update> SetOn(this InstanceSetters<Update> setters, DateTime on) =>
        setters.Set(u => u.On, on);

    public static InstanceSetters<Update> SetReason(this InstanceSetters<Update> setters, string reason) =>
        setters.Set(u => u.Reason, reason);

    public static InstanceSetters<Update> SetReleaseVersion(
        this InstanceSetters<Update> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(u => u.ReleaseVersion, releaseVersion).SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<Update> SetReleaseVersionId(
        this InstanceSetters<Update> setters,
        Guid releaseVersionId
    ) => setters.Set(u => u.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<Update> SetCreated(this InstanceSetters<Update> setters, DateTime created) =>
        setters.Set(u => u.Created, created);

    public static InstanceSetters<Update> SetCreatedById(this InstanceSetters<Update> setters, Guid createdById) =>
        setters.Set(u => u.CreatedById, createdById);
}
