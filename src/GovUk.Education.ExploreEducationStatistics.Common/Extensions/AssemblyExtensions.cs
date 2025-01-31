#nullable enable
using System.IO;
using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class AssemblyExtensions
{
    public static string GetDirectoryPath(this Assembly assembly)
        => Path.GetDirectoryName(assembly.Location)!;

    public static DirectoryInfo GetDirectory(this Assembly assembly)
        => new(assembly.GetDirectoryPath());
}
