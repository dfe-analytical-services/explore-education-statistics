using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

/// <summary>
/// Extension methods for obtaining various sizes of Tuples from a <see cref="Generator{T}"/>.
/// </summary>
public static class GeneratorTupleExtensions
{
    public static Tuple<T, T> GenerateTuple2<T>(this Generator<T> generator)
        where T : class
    {
        var list = generator.GenerateArray(2);
        return new Tuple<T, T>(list[0], list[1]);
    }

    public static Tuple<T, T, T> GenerateTuple3<T>(this Generator<T> generator)
        where T : class
    {
        var list = generator.GenerateArray(3);
        return new Tuple<T, T, T>(list[0], list[1], list[2]);
    }
}