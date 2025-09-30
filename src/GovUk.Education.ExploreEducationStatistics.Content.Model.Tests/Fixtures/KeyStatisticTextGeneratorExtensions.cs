using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class KeyStatisticTextGeneratorExtensions
{
    public static Generator<KeyStatisticText> DefaultKeyStatisticText(this DataFixture fixture) =>
        fixture.Generator<KeyStatisticText>().WithDefaults();

    public static Generator<KeyStatisticText> WithDefaults(this Generator<KeyStatisticText> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<KeyStatisticText> SetDefaults(this InstanceSetters<KeyStatisticText> setters) =>
        setters
            .SetDefault(t => t.Id)
            .SetDefault(t => t.Order)
            .SetDefault(t => t.Statistic)
            .SetDefault(t => t.Title)
            .Set(t => t.Created, f => f.Date.Past());

    public static Generator<KeyStatisticText> WithId(this Generator<KeyStatisticText> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<KeyStatisticText> WithGuidanceText(
        this Generator<KeyStatisticText> generator,
        string? guidanceText
    ) => generator.ForInstance(s => s.SetGuidanceText(guidanceText));

    public static Generator<KeyStatisticText> WithGuidanceTitle(
        this Generator<KeyStatisticText> generator,
        string? guidanceTitle
    ) => generator.ForInstance(s => s.SetGuidanceTitle(guidanceTitle));

    public static Generator<KeyStatisticText> WithOrder(this Generator<KeyStatisticText> generator, int order) =>
        generator.ForInstance(s => s.SetOrder(order));

    public static Generator<KeyStatisticText> WithStatistic(
        this Generator<KeyStatisticText> generator,
        string statistic
    ) => generator.ForInstance(s => s.SetStatistic(statistic));

    public static Generator<KeyStatisticText> WithTitle(this Generator<KeyStatisticText> generator, string title) =>
        generator.ForInstance(s => s.SetTitle(title));

    public static Generator<KeyStatisticText> WithTrend(this Generator<KeyStatisticText> generator, string? trend) =>
        generator.ForInstance(s => s.SetTrend(trend));

    public static Generator<KeyStatisticText> WithCreated(
        this Generator<KeyStatisticText> generator,
        DateTimeOffset created
    ) => generator.ForInstance(s => s.SetCreated(created));

    public static Generator<KeyStatisticText> WithCreatedById(
        this Generator<KeyStatisticText> generator,
        Guid createdById
    ) => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static Generator<KeyStatisticText> WithUpdated(
        this Generator<KeyStatisticText> generator,
        DateTimeOffset? updated
    ) => generator.ForInstance(s => s.SetUpdated(updated));

    public static Generator<KeyStatisticText> WithUpdatedById(
        this Generator<KeyStatisticText> generator,
        Guid? updatedById
    ) => generator.ForInstance(s => s.SetUpdatedById(updatedById));

    public static InstanceSetters<KeyStatisticText> SetId(this InstanceSetters<KeyStatisticText> setters, Guid id) =>
        setters.Set(o => o.Id, id);

    public static InstanceSetters<KeyStatisticText> SetGuidanceText(
        this InstanceSetters<KeyStatisticText> setters,
        string? guidanceText
    ) => setters.Set(o => o.GuidanceText, guidanceText);

    public static InstanceSetters<KeyStatisticText> SetGuidanceTitle(
        this InstanceSetters<KeyStatisticText> setters,
        string? guidanceTitle
    ) => setters.Set(o => o.GuidanceTitle, guidanceTitle);

    public static InstanceSetters<KeyStatisticText> SetOrder(
        this InstanceSetters<KeyStatisticText> setters,
        int order
    ) => setters.Set(o => o.Order, order);

    public static InstanceSetters<KeyStatisticText> SetStatistic(
        this InstanceSetters<KeyStatisticText> setters,
        string statistic
    ) => setters.Set(o => o.Statistic, statistic);

    public static InstanceSetters<KeyStatisticText> SetTitle(
        this InstanceSetters<KeyStatisticText> setters,
        string title
    ) => setters.Set(o => o.Title, title);

    public static InstanceSetters<KeyStatisticText> SetTrend(
        this InstanceSetters<KeyStatisticText> setters,
        string? trend
    ) => setters.Set(o => o.Trend, trend);

    public static InstanceSetters<KeyStatisticText> SetCreated(
        this InstanceSetters<KeyStatisticText> setters,
        DateTimeOffset created
    ) => setters.Set(o => o.Created, created);

    public static InstanceSetters<KeyStatisticText> SetCreatedById(
        this InstanceSetters<KeyStatisticText> setters,
        Guid createdById
    ) => setters.Set(o => o.CreatedById, createdById);

    public static InstanceSetters<KeyStatisticText> SetUpdated(
        this InstanceSetters<KeyStatisticText> setters,
        DateTimeOffset? updated
    ) => setters.Set(o => o.Updated, updated);

    public static InstanceSetters<KeyStatisticText> SetUpdatedById(
        this InstanceSetters<KeyStatisticText> setters,
        Guid? updatedById
    ) => setters.Set(o => o.UpdatedById, updatedById);
}
