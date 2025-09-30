#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class JsonPathUtils
{
    public static string Concat(params string[] paths) => Concat(paths.ToList());

    public static string Concat(IEnumerable<string> paths)
    {
        return paths.Aggregate(
            string.Empty,
            (acc, inputPath) =>
            {
                var path = inputPath.Trim();

                if (path == "$")
                {
                    return acc;
                }

                if (path.StartsWith("$."))
                {
                    return acc == string.Empty ? acc + path[2..] : acc + path[1..];
                }

                return acc == string.Empty ? path : $"{acc}.{path}";
            }
        );
    }
}
