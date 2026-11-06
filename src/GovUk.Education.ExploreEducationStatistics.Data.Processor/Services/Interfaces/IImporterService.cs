#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterService
{
    Task<SubjectMeta> ImportMeta(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSet dataSet,
        IStorageDataSetWriter dataSetWriter
    );

    Task ImportObservations(
        DataImport dataImport,
        Func<Task<Stream>> dataFileStreamProvider,
        Func<Task<Stream>> metaFileStreamProvider,
        IStorageDataSet dataSet,
        IStorageDataSetWriter dataSetWriter
    );

    Task ImportFiltersAndLocations(
        DataImport dataImport,
        Func<Task<Stream>> dataFileStreamProvider,
        SubjectMeta subjectMeta,
        IStorageDataSetWriter dataSetWriter
    );
}
