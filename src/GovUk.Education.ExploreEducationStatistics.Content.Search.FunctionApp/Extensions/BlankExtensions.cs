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
