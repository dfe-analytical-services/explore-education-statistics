#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseVersionGeneratorExtensions
{
    public static Generator<ReleaseVersion> DefaultStatsReleaseVersion(this DataFixture fixture) =>
        fixture.Generator<ReleaseVersion>().WithDefaults();

    public static Generator<ReleaseVersion> WithDefaults(
        this Generator<ReleaseVersion> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ReleaseVersion> WithId(
        this Generator<ReleaseVersion> generator,
        Guid id
    ) => generator.ForInstance(s => s.SetId(id));

    public static Generator<ReleaseVersion> WithPublicationId(
        this Generator<ReleaseVersion> generator,
        Guid publicationId
    ) => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static InstanceSetters<ReleaseVersion> SetDefaults(
        this InstanceSetters<ReleaseVersion> setters
    ) => setters.SetDefault(rv => rv.Id).SetDefault(rv => rv.PublicationId);

    public static InstanceSetters<ReleaseVersion> SetId(
        this InstanceSetters<ReleaseVersion> setters,
        Guid id
    ) => setters.Set(rv => rv.Id, id);

    public static InstanceSetters<ReleaseVersion> SetPublicationId(
        this InstanceSetters<ReleaseVersion> setters,
        Guid publicationId
    ) => setters.Set(rv => rv.PublicationId, publicationId);
}
