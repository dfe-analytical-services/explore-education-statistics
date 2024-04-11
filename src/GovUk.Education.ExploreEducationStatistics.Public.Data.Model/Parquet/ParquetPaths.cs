using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

public static class ParquetPaths
{
    public static string DirectoryPath(DataSetVersion dataSetVersion) => Path.Combine(
        dataSetVersion.DataSetId.ToString(),
        $"v{dataSetVersion.Version}"
    );

    public static string DataPath(DataSetVersion dataSetVersion) => Path.Combine(
        DirectoryPath(dataSetVersion),
        $"{DataTable.TableName}.parquet"
    );

    public static string FiltersPath(DataSetVersion dataSetVersion) => Path.Combine(
        DirectoryPath(dataSetVersion),
        $"{FilterOptionsTable.TableName}.parquet"
    );

    public static string LocationsPath(DataSetVersion dataSetVersion) => Path.Combine(
        DirectoryPath(dataSetVersion),
        $"{LocationOptionsTable.TableName}.parquet"
    );

    public static string TimePeriodsPath(DataSetVersion dataSetVersion) => Path.Combine(
        DirectoryPath(dataSetVersion),
        $"{TimePeriodsTable.TableName}.parquet"
    );

    public static string IndicatorsPath(DataSetVersion dataSetVersion) => Path.Combine(
        DirectoryPath(dataSetVersion),
        $"{IndicatorsTable.TableName}.parquet"
    );
}
