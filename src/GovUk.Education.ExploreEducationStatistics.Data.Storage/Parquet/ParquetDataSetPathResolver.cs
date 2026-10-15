#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

public class ParquetDataSetPathResolver : IParquetDataSetPathResolver
{
    private readonly string _basePath;

    public ParquetDataSetPathResolver(IOptions<DataFilesOptions> options, IHostEnvironment environment)
    {
        if (options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(DataFilesOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }

        _basePath = environment.IsDevelopment()
            ? Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(options.Value.BasePath))
            : options.Value.BasePath;
    }

    public string BasePath() => _basePath;
}
