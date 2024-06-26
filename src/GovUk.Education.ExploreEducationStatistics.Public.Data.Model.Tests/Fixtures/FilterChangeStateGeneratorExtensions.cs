using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterChangeStateGeneratorExtensions
{
    public static Generator<FilterChangeState> DefaultFilterChangeState(this DataFixture fixture)
        => fixture.Generator<FilterChangeState>().WithDefaults();

    public static Generator<FilterChangeState> WithDefaults(this Generator<FilterChangeState> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterChangeState> WithPublicId(
        this Generator<FilterChangeState> generator,
        string id)
        => generator.ForInstance(s => s.SetPublicId(id));

    public static Generator<FilterChangeState> WithLabel(
        this Generator<FilterChangeState> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterChangeState> WithHint(
        this Generator<FilterChangeState> generator,
        string? hint)
        => generator.ForInstance(s => s.SetHint(hint));

    public static InstanceSetters<FilterChangeState> SetDefaults(this InstanceSetters<FilterChangeState> setters)
        => setters
            .SetDefault(cs => cs.PublicId)
            .SetDefault(cs => cs.Label)
            .SetDefault(cs => cs.Hint);

    public static InstanceSetters<FilterChangeState> SetPublicId(
        this InstanceSetters<FilterChangeState> setters,
        string publicId)
        => setters.Set(cs => cs.PublicId, publicId);

    public static InstanceSetters<FilterChangeState> SetLabel(
        this InstanceSetters<FilterChangeState> setters,
        string label)
        => setters.Set(cs => cs.Label, label);

    public static InstanceSetters<FilterChangeState> SetHint(
        this InstanceSetters<FilterChangeState> setters,
        string? hint)
        => setters.Set(cs => cs.Hint, hint);
}
