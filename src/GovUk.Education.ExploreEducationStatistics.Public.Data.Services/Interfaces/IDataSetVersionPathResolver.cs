using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

public interface IDataSetVersionPathResolver
{
    string BasePath();

    string DirectoryPath(DataSetVersion dataSetVersion);

    string CsvDataPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), "data.csv.gz");

    string CsvMetadataPath(DataSetVersion dataSetVersion)
        => Path.Combine(DirectoryPath(dataSetVersion), "metadata.csv.gz");

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
