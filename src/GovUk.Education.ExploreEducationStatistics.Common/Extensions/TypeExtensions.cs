#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class TypeExtensions
{
    public static string GetPrettyFullName(this Type type)
    {
        // Remove our namespace as it really clutters up the logs
        return type.FullName?.Replace("GovUk.Education.ExploreEducationStatistics.", string.Empty)
               ?? string.Empty;
    }

    /// <summary>
    /// Returns the <see cref="Type.Name"/> property
    /// without including generic type information.
    /// </summary>
    public static string GetNameWithoutGenericArity(this Type t)
    {
        var name = t.Name;
        var index = name.IndexOf('`');

        return index == -1 ? name : name[..index];
    }

    /// <summary>
    /// Get a list of types that describes the path needed to
    /// reach an unboxed 'result' type in the type's generic
    /// arguments e.g. the right of an Either, or the result of a Task.
    /// </summary>
    /// <remarks>
    /// We currently only support 'result' types (e.g. Either, Task
    /// or ActionResult), although it may be useful to handle other
    /// types such as Lists, Dictionary, etc, in the future.
    /// </remarks>
    public static List<Type> GetUnboxedResultTypePath(this Type type)
    {
        var path = new List<Type>{ type };

        while (true)
        {
            var currentType = path.Last();

            if (!currentType.IsConstructedGenericType)
            {
                return path;
            }

            // Potentially unsafe to use just the name for this,
            // but realistically, we're never going to create
            // other types with these names.
            switch (currentType.GetNameWithoutGenericArity())
            {
                case "Task":
                case "ActionResult":
                    path.Add(currentType.GenericTypeArguments[0]);
                    continue;

                case "Either":
                    path.Add(currentType.GenericTypeArguments[1]);
                    continue;
            }

            return path;
        }
    }

    /// <summary>
    /// Get the loadable types for an assembly, ignoring types that may
    /// transitively consume types from an inaccessible assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load types from.</param>
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.WhereNotNull();
        }
    }

    /// <summary>
    /// Get the subclasses of a type.
    /// </summary>
    public static IEnumerable<Type> GetSubclasses(this Type type)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in assembly.GetLoadableTypes())
            {
                if (t.IsSubclassOf(type))
                {
                    yield return t;
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the type is considered 'simple' i.e. it can be
    /// represented as a single value e.g. a string, number, date, etc.
    /// </summary>
    public static bool IsSimple(this Type type)
    {
        var underlyingType = type.GetUnderlyingType();

        return underlyingType.IsPrimitive
            || underlyingType.IsEnum
            || underlyingType == typeof(string)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(TimeSpan)
            || underlyingType == typeof(Uri)
            || underlyingType == typeof(Guid);
    }

    /// <summary>
    /// Returns true if the type is considered 'complex' i.e. it cannot be
    /// represented as a single value e.g. an object, list, etc.
    /// This is the opposite of <see cref="IsSimple"/>.
    /// </summary>
    public static bool IsComplex(this Type type) => !type.IsSimple();

    public static bool IsNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) is not null;

    public static Type GetUnderlyingType(this Type type)
        => type.IsNullableType()
        ? Nullable.GetUnderlyingType(type)!
        : type;
}
