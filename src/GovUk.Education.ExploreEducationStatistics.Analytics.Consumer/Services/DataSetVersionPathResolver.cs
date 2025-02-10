using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;

public class AnalyticsPathResolver : IAnalyticsPathResolver
{
    private const string DraftVersionFolderName = "draft";
    
    private readonly IOptions<DataFilesOptions> _options;
    private readonly IWebHostEnvironment _environment;
    private readonly string _basePath;

    public AnalyticsPathResolver(IOptions<DataFilesOptions> options, IWebHostEnvironment environment)
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
    {
        var folderName = dataSetVersion.IsPrivateStatus() 
            ? DraftVersionFolderName
            : $"v{dataSetVersion.SemVersion()}";
        
        return Path.Combine(
            ((IDataSetVersionPathResolver) this).DataSetsPath(),
            dataSetVersion.DataSetId.ToString(),
            folderName);
    }
    
    public string DirectoryPath(DataSetVersion dataSetVersion, SemVersion versionNumber)
    {
        return Path.Combine(
            ((IDataSetVersionPathResolver) this).DataSetsPath(),
            dataSetVersion.DataSetId.ToString(),
            $"v{versionNumber}");
    }

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
