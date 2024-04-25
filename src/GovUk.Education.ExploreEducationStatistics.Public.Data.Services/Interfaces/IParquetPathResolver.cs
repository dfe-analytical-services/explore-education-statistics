using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

public interface IParquetPathResolver
{
    string DirectoryPath(DataSetVersion dataSetVersion);

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
