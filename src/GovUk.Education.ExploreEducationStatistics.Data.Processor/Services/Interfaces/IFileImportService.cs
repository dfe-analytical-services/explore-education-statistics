using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IFileImportService
{
    Task ImportObservations(DataImport import, IStorageDataSet dataSet, IStorageDataSetWriter dataSetWriter);

    Task ImportFiltersAndLocations(Guid importId, SubjectMeta subjectMeta, IStorageDataSetWriter dataSetWriter);

    Task CompleteImport(DataImport import, IStorageDataSet dataSet);
}
