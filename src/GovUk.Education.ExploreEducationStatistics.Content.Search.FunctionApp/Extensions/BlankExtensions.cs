namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class BlankExtensions
{
    public static T ThrowIfBlank<T>(this T? value, string name) 
        where T: struct
        => value ?? throw new ArgumentException($"'{name}' cannot be blank", name);
    
    public static T ThrowIfBlank<T>(this T? value, string name) 
        where T: class
        => value ?? throw new ArgumentException($"'{name}' cannot be blank", name);
}

public static class DiffExtensions
{
    public static (T[] LeftOnly, T[] Both, T[] RightOnly) Diff<T>(this IList<T> left, IList<T> right)
    {
        return (left.Except(right).ToArray(), left.Intersect(right).ToArray(), right.Except(left).ToArray());
    }
}
