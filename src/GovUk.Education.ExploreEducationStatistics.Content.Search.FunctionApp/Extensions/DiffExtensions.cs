namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class DiffExtensions
{
    public static (T[] LeftOnly, T[] Both, T[] RightOnly) Diff<T>(this IList<T> left, IList<T> right)
    {
        return (left.Except(right).ToArray(), left.Intersect(right).ToArray(), right.Except(left).ToArray());
    }
}
