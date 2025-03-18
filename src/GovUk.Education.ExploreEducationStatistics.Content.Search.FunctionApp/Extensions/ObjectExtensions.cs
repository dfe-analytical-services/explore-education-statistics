using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class ObjectExtensions
{
    public static bool IsDefault<T>([NotNullWhen(false)] this T obj) => EqualityComparer<T>.Default.Equals(obj, default);
}
