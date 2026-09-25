#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class DataFilesPathResolver(string basePath) : IDataFilesPathResolver
{
    public string ParquetV1Path(File file)
    {
        return Path.Combine(BasePath(), PathUtils.OsPath(file.ParquetV1Path()));
    }

    private string BasePath()
    {
        if (basePath.IsNullOrWhitespace())
        {
            throw new InvalidOperationException("A base path for data files must be configured");
        }

        return Path.IsPathRooted(basePath)
            ? basePath
            : Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(basePath));
    }
}
