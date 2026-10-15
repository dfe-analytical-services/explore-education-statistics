#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;

public interface IParquetDataSetPathResolver
{
    public const string DataFilesDirectoryName = "data-files";
    public const string StagingDbFilename = "data.db";

    string BasePath();

    string DirectoryPath(Guid subjectId) => Path.Combine(BasePath(), DataFilesDirectoryName, subjectId.ToString());

    string StagingDbPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), StagingDbFilename);

    string DataPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), DataTable.ParquetFile);

    string FilterOptionsPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), FilterOptionsTable.ParquetFile);

    string FiltersPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), FiltersTable.ParquetFile);

    string IndicatorsPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), IndicatorsTable.ParquetFile);

    string LocationOptionsPath(Guid subjectId) =>
        Path.Combine(DirectoryPath(subjectId), LocationOptionsTable.ParquetFile);

    string LocationsPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), LocationsTable.ParquetFile);

    string TimePeriodsPath(Guid subjectId) => Path.Combine(DirectoryPath(subjectId), TimePeriodsTable.ParquetFile);
}
