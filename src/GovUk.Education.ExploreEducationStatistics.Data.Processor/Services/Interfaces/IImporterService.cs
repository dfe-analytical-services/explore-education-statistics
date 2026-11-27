#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterService
{
    Task ImportObservations(
        DataImport dataImport,
        Func<Task<Stream>> dataFileStreamProvider,
        IStorageDataSetWriter dataSetWriter
    );

    Task ImportFiltersAndLocations(
        DataImport dataImport,
        Func<Task<Stream>> dataFileStreamProvider,
        List<Filter> filters,
        IStorageDataSetWriter dataSetWriter
    );
}
