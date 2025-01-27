using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

public interface IDataSetVersionPathResolver
{
    string BasePath();

    string DataSetsPath() => Path.Combine(BasePath(), DataSetFilenames.DataSetsDirectory);

    string DirectoryPath(DataSetVersion dataSetVersion);

    // TODO EES-5660 - remove after draft DataSetVersion folders are migrated for each
    // Public API-enabled environment.
    string DirectoryPath(DataSetVersion dataSetVersion, SemVersion versionNumber);

    string CsvDataPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataSetFilenames.CsvDataFile);

    string CsvMetadataPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataSetFilenames.CsvMetadataFile);

    string DuckDbPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataSetFilenames.DuckDbDatabaseFile);

    string DuckDbLoadSqlPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataSetFilenames.DuckDbLoadSqlFile);

    string DuckDbSchemaSqlPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataSetFilenames.DuckDbSchemaSqlFile);

    string DataPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), DataTable.ParquetFile);

    string FiltersPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), FilterOptionsTable.ParquetFile);

    string IndicatorsPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), IndicatorsTable.ParquetFile);

    string LocationsPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), LocationOptionsTable.ParquetFile);

    string TimePeriodsPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), TimePeriodsTable.ParquetFile);
}
