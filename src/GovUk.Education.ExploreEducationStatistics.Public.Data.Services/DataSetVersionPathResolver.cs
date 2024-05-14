using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services;

public class DataSetVersionPathResolver : IDataSetVersionPathResolver
{
    private readonly IOptions<ParquetFilesOptions> _options;
    private readonly IWebHostEnvironment _environment;

    private readonly string _basePath;

    public DataSetVersionPathResolver(IOptions<ParquetFilesOptions> options, IWebHostEnvironment environment)
    {
        _options = options;
        _environment = environment;

        if (_options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(ParquetFilesOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }

        _basePath = GetBasePath();
    }

    public string DirectoryPath(DataSetVersion dataSetVersion)
        => Path.Combine(_basePath, dataSetVersion.DataSetId.ToString(), $"v{dataSetVersion.Version}");

    private string GetBasePath()
    {
        if (_environment.IsDevelopment())
        {
            return Path.Combine(PathUtils.ProjectRootPath, PathUtils.OsPath(_options.Value.BasePath));
        }

        if (_environment.IsIntegrationTest())
        {
            return Path.Combine(
                Assembly.GetExecutingAssembly().GetDirectoryPath(),
                PathUtils.OsPath(_options.Value.BasePath)
            );
        }

        return _options.Value.BasePath;
    }
}
