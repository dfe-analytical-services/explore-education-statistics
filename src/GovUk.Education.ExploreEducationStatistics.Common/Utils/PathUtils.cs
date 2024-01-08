#nullable enable
using System;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class PathUtils
{
    private const string SolutionFilename = "GovUk.Education.ExploreEducationStatistics.sln";

    private static readonly Lazy<string> ProjectRootPathLazy = new(
        () =>
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
        }
    );

    public static string ProjectRootPath => ProjectRootPathLazy.Value;
}
