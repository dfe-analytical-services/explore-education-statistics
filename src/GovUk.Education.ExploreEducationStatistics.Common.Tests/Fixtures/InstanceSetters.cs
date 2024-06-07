#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bogus;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class InstanceSetters<T> where T : class
{
    private readonly Action<string, Func<Faker, T, SetterContext, object?>> _onProperty;
    private readonly Action<Action<Faker, T, SetterContext>> _onAction;

    public InstanceSetters(
        Action<string, Func<Faker, T, SetterContext, object?>> onProperty,
        Action<Action<Faker, T, SetterContext>> onAction)
    {
        _onProperty = onProperty;
        _onAction = onAction;
    }

    public InstanceSetters<T> Set<TProperty>(Expression<Func<T, TProperty>> property, TProperty value) =>
        Set(property, () => value);

    public InstanceSetters<T> Set<TProperty>(Expression<Func<T, TProperty>> property, Func<TProperty> value) =>
        Set(property, _ => value());

    public InstanceSetters<T> Set<TProperty>(
        Expression<Func<T, TProperty>> property,
        Func<Faker, TProperty> setter)
        => Set(property, (faker, _, _) => setter(faker));

    public InstanceSetters<T> Set<TProperty>(
        Expression<Func<T, TProperty>> property,
        Func<Faker, T, TProperty> setter)
        => Set(property, (faker, instance, _) => setter(faker, instance));

    /// <summary>
    /// Set the value for a property on the instance <see cref="T"/>.
    /// </summary>
    /// <param name="property">The name of the property to set.</param>
    /// <param name="setter">The function to set the property's value.</param>
    public InstanceSetters<T> Set<TProperty>(
        Expression<Func<T, TProperty>> property,
        Func<Faker, T, SetterContext, TProperty> setter)
    {
        _onProperty(
            PropertyName.For(property),
            (faker, instance, context) => setter(faker, instance, context)
        );
        return this;
    }

    public InstanceSetters<T> Set(Action<Faker, T> setter) =>
        Set((faker, instance, _) => setter(faker, instance));

    /// <summary>
    /// Set multiple properties on the instance of <see cref="T"/>.
    /// </summary>
    /// <param name="setter">The setter action.</param>
    public InstanceSetters<T> Set(Action<Faker, T, SetterContext> setter)
    {
        _onAction(setter);
        return this;
    }

    // Default setters

    public InstanceSetters<T> SetDefault(Expression<Func<T, Guid>> property)
        => Set(property, faker => faker.Random.Guid());

    public InstanceSetters<T> SetDefault(Expression<Func<T, Guid?>> property)
        => Set(property, faker => faker.Random.Guid());

    public InstanceSetters<T> SetDefault(Expression<Func<T, string?>> property)
    {
        return Set(
            property,
            (faker, _, context) =>
            {
                var index = GetDisplayIndex(context, faker);

                return $"{typeof(T).Name} {index} :: {PropertyName.For(property)}";
            }
        );
    }

    public InstanceSetters<T> SetDefault<TEnum>(Expression<Func<T, TEnum>> property) where TEnum : struct, Enum
        => Set(property, faker => faker.PickRandom<TEnum>());

    public InstanceSetters<T> SetDefault<TEnum>(Expression<Func<T, TEnum?>> property) where TEnum : struct, Enum
        => Set(property, faker => faker.PickRandom<TEnum>());

    public InstanceSetters<T> SetDefault(Expression<Func<T, IList<string>>> property)
    {
        return Set(
            property,
            (faker, _, context) =>
            {
                var index = GetDisplayIndex(context, faker);

                return Enumerable.Range(0, faker.Random.Int(2, 5))
                    .Select(itemIndex =>
                        $"{typeof(T).Name} {index} :: {PropertyName.For(property)} :: Item {itemIndex}")
                    .ToList();
            }
        );
    }

    public InstanceSetters<T> SetDefault<TEnum>(Expression<Func<T, IList<TEnum>>> property) where TEnum : Enum
        => Set(
            property,
            faker => EnumUtil.GetEnums<TEnum>()
                .Take(faker.Random.Int(2, 5))
                .ToList());

    public InstanceSetters<T> SetDefault(Expression<Func<T, int?>> property, int? offset = 0)
    {
        return Set(
            property,
            (faker, _, context) => offset + GetDisplayIndex(context, faker)
        );
    }

    public InstanceSetters<T> SetDefault(Expression<Func<T, DateTime?>> property)
    {
        return Set(property, DateTime.UtcNow.AddDays(-1));
    }

    public int GetDisplayIndex(SetterContext context, Faker faker)
        => context.FixtureTypeIndex > 0 ? context.FixtureTypeIndex : faker.IndexFaker;
}

/// <summary>
/// Provides the setter with contextual data whilst generating instances.
/// </summary>
public record SetterContext
{
    /// <summary>
    /// The instance's index in the generated sequence.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// The associated fixture for this generator (if there is one).
    /// If no associated fixture, this will be null.
    /// </summary>
    public readonly DataFixture? Fixture;

    private readonly int? _fixtureTypeIndex;
    private readonly int? _fixtureIndex;

    public SetterContext(int index, DataFixture? fixture, int? fixtureTypeIndex, int? fixtureIndex)
    {
        Index = index;
        Fixture = fixture;
        _fixtureTypeIndex = fixtureTypeIndex;
        _fixtureIndex = fixtureIndex;
    }

    /// <summary>
    /// The instance's index across all generators for this type in the
    /// associated fixture. If no associated fixture, this will be -1.
    /// </summary>
    public int FixtureTypeIndex => _fixtureTypeIndex ?? -1;

    /// <summary>
    /// The instance's index across all instances generated by the
    /// associated fixture. If no associated fixture, this will be -1.
    /// </summary>
    public int FixtureIndex => _fixtureIndex ?? -1;
}
