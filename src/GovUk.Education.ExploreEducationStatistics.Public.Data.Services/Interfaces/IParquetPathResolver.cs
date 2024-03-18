using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

public interface IParquetPathResolver
{
    string DirectoryPath(DataSetVersion dataSetVersion);

    string DataPath(DataSetVersion dataSetVersion);

    string FiltersPath(DataSetVersion dataSetVersion);

    string LocationsPath(DataSetVersion dataSetVersion);

    string TimePeriodsPath(DataSetVersion dataSetVersion);
}
