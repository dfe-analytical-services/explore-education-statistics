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
    private readonly IOptions<DataFilesOptions> _options;
    private readonly IWebHostEnvironment _environment;
    private readonly string _basePath;

    public DataSetVersionPathResolver(IOptions<DataFilesOptions> options, IWebHostEnvironment environment)
    {
        _options = options;
        _environment = environment;

        if (_options.Value.BasePath.IsNullOrWhitespace())
        {
            throw new ArgumentException(
                message: $"'{nameof(DataFilesOptions.BasePath)}' must not be blank",
                paramName: nameof(options)
            );
        }

        _basePath = GetBasePath();
    }

    public string BasePath() => _basePath;

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
            var randomTestInstanceDir = Guid.NewGuid().ToString();
            return Path.Combine(
                Path.GetTempPath(),
                "ExploreEducationStatistics",
                PathUtils.OsPath(_options.Value.BasePath),
                randomTestInstanceDir
            );
        }

        return _options.Value.BasePath;
    }
}
