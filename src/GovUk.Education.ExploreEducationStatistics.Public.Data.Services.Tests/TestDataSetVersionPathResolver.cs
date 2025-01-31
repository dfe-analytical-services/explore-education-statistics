using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;

public class TestDataSetVersionPathResolver : IDataSetVersionPathResolver
{
    private readonly string _basePath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "DataFiles"
    );

    public string BasePath() => _basePath;

    public string Directory { get; set; } = string.Empty;

    public string DirectoryPath(DataSetVersion dataSetVersion)
    {
        return Path.Combine(_basePath, Directory);
    }

    public string DirectoryPath(DataSetVersion dataSetVersion, SemVersion versionNumber)
    {
        return Path.Combine(_basePath, Directory);
    }
}
