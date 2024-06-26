using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionChangeStateGeneratorExtensions
{
    public static Generator<LocationOptionChangeState> DefaultLocationOptionChangeState(this DataFixture fixture)
        => fixture.Generator<LocationOptionChangeState>().WithDefaults();

    public static Generator<LocationOptionChangeState> WithDefaults(this Generator<LocationOptionChangeState> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionChangeState> WithId(
        this Generator<LocationOptionChangeState> generator,
        string id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<LocationOptionChangeState> WithLabel(
        this Generator<LocationOptionChangeState> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<LocationOptionChangeState> WithCode(
        this Generator<LocationOptionChangeState> generator,
        string filterId)
        => generator.ForInstance(s => s.SetCode(filterId));

    public static InstanceSetters<LocationOptionChangeState> SetDefaults(
        this InstanceSetters<LocationOptionChangeState> setters)
        => setters
            .SetDefault(cs => cs.Id)
            .SetDefault(cs => cs.Label)
            .SetDefault(cs => cs.Code);

    public static InstanceSetters<LocationOptionChangeState> SetId(
        this InstanceSetters<LocationOptionChangeState> setters,
        string id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<LocationOptionChangeState> SetLabel(
        this InstanceSetters<LocationOptionChangeState> setters,
        string label)
        => setters.Set(cs => cs.Label, label);

    public static InstanceSetters<LocationOptionChangeState> SetCode(
        this InstanceSetters<LocationOptionChangeState> setters,
        string filterId)
        => setters.Set(cs => cs.Code, filterId);
}
