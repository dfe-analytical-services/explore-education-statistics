using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IFileImportService
{
    Task ImportObservations(DataImport import, IStorageDataSetWriter dataSetWriter);

    Task ImportFiltersAndLocations(Guid importId, List<Filter> filters, IStorageDataSetWriter dataSetWriter);

    Task CompleteImport(DataImport import, IStorageDataSetWriter dataSetWriter);
}
