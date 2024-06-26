using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionChangeStateGeneratorExtensions
{
    public static Generator<FilterOptionChangeState> DefaultFilterOptionChangeState(this DataFixture fixture)
        => fixture.Generator<FilterOptionChangeState>().WithDefaults();

    public static Generator<FilterOptionChangeState> WithDefaults(this Generator<FilterOptionChangeState> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionChangeState> WithPublicId(
        this Generator<FilterOptionChangeState> generator,
        string id)
        => generator.ForInstance(s => s.SetPublicId(id));

    public static Generator<FilterOptionChangeState> WithLabel(
        this Generator<FilterOptionChangeState> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterOptionChangeState> WithFilterId(
        this Generator<FilterOptionChangeState> generator,
        string filterId)
        => generator.ForInstance(s => s.SetFilterId(filterId));

    public static Generator<FilterOptionChangeState> WithIsAggregate(
        this Generator<FilterOptionChangeState> generator,
        bool? isAggregate)
        => generator.ForInstance(s => s.SetIsAggregate(isAggregate));

    public static InstanceSetters<FilterOptionChangeState> SetDefaults(this InstanceSetters<FilterOptionChangeState> setters)
        => setters
            .SetDefault(cs => cs.PublicId)
            .SetDefault(cs => cs.Label)
            .SetDefault(cs => cs.FilterId);

    public static InstanceSetters<FilterOptionChangeState> SetPublicId(
        this InstanceSetters<FilterOptionChangeState> setters,
        string id)
        => setters.Set(cs => cs.PublicId, id);

    public static InstanceSetters<FilterOptionChangeState> SetLabel(
        this InstanceSetters<FilterOptionChangeState> setters,
        string label)
        => setters.Set(cs => cs.Label, label);

    public static InstanceSetters<FilterOptionChangeState> SetFilterId(
        this InstanceSetters<FilterOptionChangeState> setters,
        string filterId)
        => setters.Set(cs => cs.FilterId, filterId);
    
    public static InstanceSetters<FilterOptionChangeState> SetIsAggregate(
        this InstanceSetters<FilterOptionChangeState> setters,
        bool? isAggregate)
        => setters.Set(cs => cs.IsAggregate, isAggregate);
}
