using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;

public class TestDataSetVersionPathResolver : IDataSetVersionPathResolver
{
    private string BasePath { get; set; } = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "ParquetFiles"
    );

    public string Directory { get; set; } = string.Empty;

    public string DirectoryPath(DataSetVersion dataSetVersion) => Path.Combine(BasePath, Directory);
}
