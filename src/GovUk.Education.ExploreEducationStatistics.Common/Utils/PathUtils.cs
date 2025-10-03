#nullable enable
using System.Reflection;
using System.Runtime.InteropServices;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class PathUtils
{
    private const string SolutionFilename = "GovUk.Education.ExploreEducationStatistics.sln";

    private static readonly Lazy<string> ProjectRootPathLazy = new(() =>
    {
        var directory = Assembly.GetExecutingAssembly().GetDirectory();

        while (directory is not null && directory.GetFiles(SolutionFilename).Length == 0)
        {
            directory = directory.Parent;
        }

        if (directory?.Parent is null)
        {
            throw new Exception("Could not detect project root");
        }

        return directory.Parent.FullName;
    });

    public static string ProjectRootPath => ProjectRootPathLazy.Value;

    /// <summary>
    /// Convert a path to the correct format for the target OS (defaulting to the current OS).
    /// </summary>
    /// <param name="path">The path to convert. Can be Windows or Posix style.</param>
    /// <param name="osPlatform">Specify the target OS to convert the path for. Defaults to the current OS.</param>
    /// <returns>A path compatible with the target OS</returns>
    public static string OsPath(string path, OSPlatform? osPlatform = null)
    {
        if (
            osPlatform == OSPlatform.Windows
            || (osPlatform == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        )
        {
            return path.Replace('/', '\\');
        }

        var replacedPath = path.Replace('\\', '/');

        if (path.Length < 3)
        {
            return replacedPath;
        }

        // This is a drive path e.g. C:\blah, so try and convert it
        // to something Posix compatible i.e. /c/blah.
        if (char.IsLetter(replacedPath[0]) && replacedPath[1] == ':')
        {
            var drive = char.ToLowerInvariant(replacedPath[0]);
            var remainingPath = replacedPath[2] == '/' ? replacedPath[3..] : replacedPath[2..];

            return $"/{drive}/{remainingPath}";
        }

        return replacedPath;
    }
}
