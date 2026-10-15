#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils;

public class TestParquetDataSetPathResolver(string basePath) : IParquetDataSetPathResolver
{
    public string BasePath() => basePath;
}
