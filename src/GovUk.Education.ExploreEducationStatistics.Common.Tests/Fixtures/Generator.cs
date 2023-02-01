#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bogus;
using Bogus.Extensions;
using Binder = Bogus.Binder;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class Generator<T> where T : class
{
    private readonly Faker _faker;
    private readonly Func<int>? _seeder;
    private readonly DataFixture? _fixture;

    private Func<T> _factory = Activator.CreateInstance<T>;

    // Setters
    private readonly List<ISetter> _setters = new();
    private readonly List<RangeSetter> _rangeSetters = new();

    private readonly List<Action<T, Faker>> _finalizers = new();

    // Holds reflection metadata about properties on T
    private static readonly Lazy<Dictionary<string, MemberInfo>> TypeProperties =
        new(() => new Binder().GetMembers(typeof(T)));

    // Performance optimisation to avoid excessive reflection during property setting
    private static readonly Dictionary<string, Action<T, object?>> SetterCache = new();

    public Generator(
        Faker? faker = null,
        Func<int>? seeder = null,
        DataFixture? fixture = null)
    {
        _faker = faker ?? new Faker();
        _seeder = seeder;
        _fixture = fixture;
    }

    /// <summary>
    /// Set a different factory function for instantiating an instance
    /// of <see cref="T"/>. Defaults to <see cref="Activator.CreateInstance{T}"/>.
    /// </summary>
    /// <param name="factory">The factory function to instantiate T.</param>
    public Generator<T> InstantiateWith(Func<T> factory)
    {
        _factory = factory;
        return this;
    }

    /// <summary>
    /// Set properties on the instance of <see cref="T"/>.
    /// </summary>
    /// <remarks>
    /// Setters will be applied in the order that they are registered, with
    /// newer setters overriding older setters.
    /// </remarks>
    /// <param name="builder">A builder function for registering setters.</param>
    public Generator<T> ForInstance(Action<InstanceSetters<T>> builder)
    {
        builder(
            new InstanceSetters<T>(
                onAction: setter =>
                    _setters.Add(new ActionSetter(setter)),
                onProperty: (property, setter) =>
                    _setters.Add(new PropertySetter(property, setter))
            )
        );

        return this;
    }

    /// <summary>
    /// Set properties on the instance of <see cref="T"/> over a range of values
    /// e.g. the first 2, last 2, etc.
    /// </summary>
    /// <remarks>
    /// Range setters will take precedence over <see cref="ForInstance"/> setters.
    /// If a range applies, it will override any previous instance setters.
    /// </remarks>
    /// <param name="range">The range of indices that the setters apply.</param>
    /// <param name="builder">A builder for registering setters for the range.</param>
    public Generator<T> ForRange(Range range, Action<InstanceSetters<T>> builder)
    {
        _rangeSetters.Add(new RangeSetter(range, builder));
        return this;
    }

    public Generator<T> FinishWith(Action<T> action) => FinishWith((instance, _) => action(instance));

    /// <summary>
    /// Apply an action after the instance <see cref="T"/> has been
    /// generated and all other setters have been executed.
    /// </summary>
    /// <param name="action">The finalising action.</param>
    public Generator<T> FinishWith(Action<T, Faker> action)
    {
        _finalizers.Add(action);
        return this;
    }

    /// <summary>
    /// Generate a single instance of <see cref="T"/>.
    /// </summary>
    public T Generate()
    {
        var instance = GenerateSingle(
            index: 0,
            fixtureTypeIndex: NextFixtureTypeIndex(),
            fixtureIndex: NextFixtureIndex());

        InvokeFinalizers(instance);

        return instance;
    }

    /// <summary>
    /// Automatically generate an instance of T from the generator
    /// using implicit type conversion.
    /// </summary>
    public static implicit operator T(Generator<T> generator)
    {
        return generator.Generate();
    }

    /// <summary>
    /// Generate multiple instances of <see cref="T"/>.
    /// </summary>
    /// <param name="count">The number of instances.</param>
    public IEnumerable<T> Generate(int count) =>
        Enumerable.Range(1, count)
            .Select(i => GenerateWithRange(index: i - 1, length: count));

    public List<T> GenerateList(int count) => Generate(count).ToList();

    public T[] GenerateArray(int count) => Generate(count).ToArray();

    private T GenerateSingle(int index, int? fixtureTypeIndex, int? fixtureIndex)
    {
        if (_seeder is not null)
        {
            _faker.Random = new Randomizer(_seeder());
        }

        var instance = _factory();

        Interlocked.Increment(ref _faker.IndexFaker);

        foreach (var setter in _setters)
        {
            setter.Invoke(
                _faker,
                instance,
                new SetterContext(
                    index: index,
                    fixtureTypeIndex: fixtureTypeIndex,
                    fixtureIndex: fixtureIndex
                )
            );
        }

        return instance;
    }

    private T GenerateWithRange(int index, int length)
    {
        var fixtureTypeIndex = NextFixtureTypeIndex();
        var fixtureIndex = NextFixtureIndex();

        var instance = GenerateSingle(
            index: index,
            fixtureTypeIndex: fixtureTypeIndex,
            fixtureIndex: fixtureIndex);

        // Apply any range setters. These take precedence over the non-ranged setters.
        foreach (var rangeSetter in _rangeSetters)
        {
            var (rangeOffset, rangeLength) = rangeSetter.Range.GetOffsetAndLength(length);

            if (rangeLength == 0 && rangeOffset != index)
            {
                continue;
            }

            var rangeEnd = rangeOffset + rangeLength;

            // Ranges are not inclusive of the end index
            if (rangeLength > 0 && (index < rangeOffset || index >= rangeEnd))
            {
                continue;
            }

            var setters = new List<ISetter>();

            rangeSetter.Builder(
                new InstanceSetters<T>(
                    onAction: setter =>
                        setters.Add(new ActionSetter(setter)),
                    onProperty: (property, setter) =>
                        setters.Add(new PropertySetter(property, setter))
                )
            );

            foreach (var setter in setters)
            {
                setter.Invoke(
                    _faker,
                    instance,
                    new SetterContext(
                        index: index,
                        fixtureTypeIndex: fixtureTypeIndex,
                        fixtureIndex: fixtureIndex
                    )
                );
            }
        }

        InvokeFinalizers(instance);

        return instance;
    }

    private void InvokeFinalizers(T instance) =>
        _finalizers.ForEach(finalizer => finalizer(instance, _faker));

    private int? NextFixtureTypeIndex() => _fixture?.NextTypeIndex<T>();

    private int? NextFixtureIndex() => _fixture?.NextIndex();

    private interface ISetter
    {
        void Invoke(Faker faker, T instance, SetterContext context);
    }

    private record ActionSetter(Action<Faker, T, SetterContext> Action) : ISetter
    {
        public void Invoke(Faker faker, T instance, SetterContext context)
            => Action(faker, instance, context);
    }

    private record PropertySetter(
        string Property,
        Func<Faker, T, SetterContext, object?> Setter) : ISetter
    {
        public void Invoke(Faker faker, T instance, SetterContext context)
        {
            var value = Setter(faker, instance, context);

            if (SetterCache.TryGetValue(Property, out var setter))
            {
                setter(instance, value);
                return;
            }

            if (!TypeProperties.Value.TryGetValue(Property, out var member))
            {
                return;
            }

            setter = member switch
            {
                PropertyInfo prop => prop.CreateSetter<T>(),
                FieldInfo field => field.SetValue,
                _ => null
            };

            if (setter is null)
            {
                return;
            }

            SetterCache.Add(Property, setter);
            setter(instance, value);
        }
    }

    private record RangeSetter(Range Range, Action<InstanceSetters<T>> Builder);
}
