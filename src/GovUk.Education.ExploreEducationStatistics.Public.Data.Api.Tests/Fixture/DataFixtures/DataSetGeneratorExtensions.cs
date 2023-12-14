#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
public static class DataSetGeneratorExtensions
{
    public static Generator<DataSet> DefaultDataSet(this DataFixture fixture)
        => fixture.Generator<DataSet>().WithDefaults();

    public static Generator<DataSet> WithDefaults(this Generator<DataSet> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<DataSet> SetDefaults(this InstanceSetters<DataSet> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Title)
            .SetDefault(f => f.Summary)
            .SetDefault(f => f.PublicationId)
            .Set(f => f.Status, DataSetStatus.Staged)
            .Set(f => f.Created, f => f.Date.PastOffset())
            .Set(
                f => f.Updated,
                (f, dataSet) => f.Date.SoonOffset(14, dataSet.Created)
            );

    public static Generator<DataSet> WithPublicationId(this Generator<DataSet> generator, Guid publicationId)
        => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static Generator<DataSet> WithStatus(this Generator<DataSet> generator, DataSetStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static InstanceSetters<DataSet> SetPublicationId(
        this InstanceSetters<DataSet> instanceSetter, 
        Guid publicationId)
        => instanceSetter.Set(f => f.PublicationId, publicationId);

    public static InstanceSetters<DataSet> SetStatus(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetStatus status)
        => instanceSetter.Set(f => f.Status, status);
}
