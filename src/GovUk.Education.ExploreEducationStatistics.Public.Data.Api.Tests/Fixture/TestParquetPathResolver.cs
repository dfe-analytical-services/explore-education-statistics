using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public class TestParquetPathResolver : IParquetPathResolver
{
    public string BasePath { get; set; } = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "ParquetFiles"
    );

    public string Directory { get; set; } = string.Empty;

    public string DirectoryPath(DataSetVersion dataSetVersion) => Path.Combine(BasePath, Directory);
}
