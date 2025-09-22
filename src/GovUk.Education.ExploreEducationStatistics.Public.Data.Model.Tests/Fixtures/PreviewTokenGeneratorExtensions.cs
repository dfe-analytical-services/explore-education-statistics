using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class PreviewTokenGeneratorExtensions
{
    public static Generator<PreviewToken> DefaultPreviewToken(this DataFixture fixture, bool activated = true, bool expired = false) => 
        fixture.Generator<PreviewToken>().WithDefaults(activated, expired);

    public static Generator<PreviewToken> WithDefaults(this Generator<PreviewToken> generator, bool activated = true, bool expired = false) => 
        generator.ForInstance(s => s.SetDefaults(activated, expired));

    public static Generator<PreviewToken> WithDataSetVersion(
        this Generator<PreviewToken> generator,
        DataSetVersion dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<PreviewToken> WithDataSetVersionId(
        this Generator<PreviewToken> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<PreviewToken> WithCreatedByUserId(
        this Generator<PreviewToken> generator,
        Guid createdByUserId
    ) => generator.ForInstance(s => s.SetCreatedByUserId(createdByUserId));

    public static Generator<PreviewToken> WithCreated(this Generator<PreviewToken> generator, DateTimeOffset created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<PreviewToken> WithExpiry(this Generator<PreviewToken> generator, DateTimeOffset expiry) =>
        generator.ForInstance(s => s.SetExpiry(expiry));

    public static Generator<PreviewToken> WithLabel(this Generator<PreviewToken> generator, string label) =>
        generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<PreviewToken> SetDefaults(
        this InstanceSetters<PreviewToken> setters,
        bool activated = true,
        bool expired = false
    ) =>
        setters
            .SetDefault(pt => pt.Id)
            .SetDefault(pt => pt.Label)
            .SetDefault(pt => pt.DataSetVersionId)
            .SetDefault(pt => pt.CreatedByUserId)
            .Set(pt => pt.Expiry, expired ? DateTimeOffset.UtcNow.AddSeconds(-1) : DateTimeOffset.UtcNow.AddDays(1)) 
            .Set(pt => pt.Activates, activated ? DateTimeOffset.UtcNow.AddSeconds(-1) : DateTimeOffset.UtcNow.AddDays(1));

    public static InstanceSetters<PreviewToken> SetDataSetVersion(
        this InstanceSetters<PreviewToken> instanceSetter,
        DataSetVersion dataSetVersion
    ) => instanceSetter.Set(pt => pt.DataSetVersion, dataSetVersion).Set(pt => pt.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<PreviewToken> SetDataSetVersionId(
        this InstanceSetters<PreviewToken> instanceSetter,
        Guid dataSetVersionId
    ) => instanceSetter.Set(pt => pt.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<PreviewToken> SetCreatedByUserId(
        this InstanceSetters<PreviewToken> instanceSetter,
        Guid createdByUserId
    ) => instanceSetter.Set(pt => pt.CreatedByUserId, createdByUserId);

    public static InstanceSetters<PreviewToken> SetCreated(
        this InstanceSetters<PreviewToken> instanceSetter,
        DateTimeOffset created
    ) => instanceSetter.Set(pt => pt.Created, created);

    public static InstanceSetters<PreviewToken> SetExpiry(
        this InstanceSetters<PreviewToken> instanceSetter,
        DateTimeOffset expiry
    ) => instanceSetter.Set(pt => pt.Expires, expiry);

    public static InstanceSetters<PreviewToken> SetLabel(
        this InstanceSetters<PreviewToken> instanceSetter,
        string label
    ) => instanceSetter.Set(pt => pt.Label, label);
}
