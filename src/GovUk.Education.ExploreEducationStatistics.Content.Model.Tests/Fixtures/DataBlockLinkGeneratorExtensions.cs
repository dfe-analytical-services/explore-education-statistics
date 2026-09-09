using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockVersionLinkGeneratorExtensions
{
    public static Generator<DataBlockVersionLink> DefaultDataVersionBlockVersionLink(this DataFixture fixture) =>
        fixture.Generator<DataBlockVersionLink>().WithDefaults();

    public static Generator<DataBlockVersionLink> WithDefaults(this Generator<DataBlockVersionLink> generator) =>
        generator.ForInstance(dbl => dbl.SetDefaults());

    public static Generator<DataBlockVersionLink> WithId(this Generator<DataBlockVersionLink> generator, Guid id) =>
        generator.ForInstance(dbl => dbl.SetId(id));

    public static Generator<DataBlockVersionLink> WithDataBlockVersion(
        this Generator<DataBlockVersionLink> generator,
        DataBlockVersion dataBlockVersion
    ) => generator.ForInstance(dbl => dbl.SetDataBlockVersion(dataBlockVersion));

    public static Generator<DataBlockVersionLink> WithComments(
        this Generator<DataBlockVersionLink> generator,
        List<Comment> comments
    ) => generator.ForInstance(dbl => dbl.SetComments(comments));

    public static Generator<DataBlockVersionLink> WithOrder(
        this Generator<DataBlockVersionLink> generator,
        int order
    ) => generator.ForInstance(dbl => dbl.SetOrder(order));

    public static Generator<DataBlockVersionLink> WithReleaseVersion(
        this Generator<DataBlockVersionLink> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(dbl => dbl.SetReleaseVersion(releaseVersion));

    public static Generator<DataBlockVersionLink> WithReleaseVersionId(
        this Generator<DataBlockVersionLink> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(dbl => dbl.SetReleaseVersionId(releaseVersionId));

    public static Generator<DataBlockVersionLink> WithContentSection(
        this Generator<DataBlockVersionLink> generator,
        ContentSection contentSection
    ) => generator.ForInstance(dbl => dbl.SetContentSection(contentSection));

    public static Generator<DataBlockVersionLink> WithCreated(
        this Generator<DataBlockVersionLink> generator,
        DateTime created
    ) => generator.ForInstance(dbl => dbl.SetCreated(created));

    public static Generator<DataBlockVersionLink> WithUpdated(
        this Generator<DataBlockVersionLink> generator,
        DateTime? updated
    ) => generator.ForInstance(dbl => dbl.SetUpdated(updated));

    public static InstanceSetters<DataBlockVersionLink> SetDefaults(
        this InstanceSetters<DataBlockVersionLink> setters
    ) =>
        setters
            .SetDefault(dbl => dbl.Id)
            .SetDefault(dbl => dbl.Order, offset: 1)
            .Set(dbl => dbl.Created, f => f.Date.Past());

    public static InstanceSetters<DataBlockVersionLink> SetId(
        this InstanceSetters<DataBlockVersionLink> setters,
        Guid id
    ) => setters.Set(dbl => dbl.Id, id);

    // A DataBlockVersionLink shares its Id with the DataBlockVersion it points at (see DataBlockService/amendment code).
    public static InstanceSetters<DataBlockVersionLink> SetDataBlockVersion(
        this InstanceSetters<DataBlockVersionLink> setters,
        DataBlockVersion dataBlockVersion
    ) =>
        setters
            .Set(dbl => dbl.Id, dataBlockVersion.Id)
            .Set(dbl => dbl.DataBlockVersion, dataBlockVersion)
            .Set(dbl => dbl.DataBlockVersionId, dataBlockVersion.Id);

    public static InstanceSetters<DataBlockVersionLink> SetComments(
        this InstanceSetters<DataBlockVersionLink> setters,
        List<Comment> comments
    ) => setters.Set(dbl => dbl.Comments, comments);

    public static InstanceSetters<DataBlockVersionLink> SetOrder(
        this InstanceSetters<DataBlockVersionLink> setters,
        int order
    ) => setters.Set(dbl => dbl.Order, order);

    public static InstanceSetters<DataBlockVersionLink> SetReleaseVersion(
        this InstanceSetters<DataBlockVersionLink> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(dbl => dbl.ReleaseVersion, releaseVersion).SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<DataBlockVersionLink> SetReleaseVersionId(
        this InstanceSetters<DataBlockVersionLink> setters,
        Guid releaseVersionId
    ) => setters.Set(dbl => dbl.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<DataBlockVersionLink> SetContentSection(
        this InstanceSetters<DataBlockVersionLink> setters,
        ContentSection contentSection
    ) => setters.Set(dbl => dbl.ContentSection, contentSection).Set(dbl => dbl.ContentSectionId, contentSection.Id);

    public static InstanceSetters<DataBlockVersionLink> SetCreated(
        this InstanceSetters<DataBlockVersionLink> setters,
        DateTime created
    ) => setters.Set(dbl => dbl.Created, created);

    public static InstanceSetters<DataBlockVersionLink> SetUpdated(
        this InstanceSetters<DataBlockVersionLink> setters,
        DateTime? updated
    ) => setters.Set(dbl => dbl.Updated, updated);
}
