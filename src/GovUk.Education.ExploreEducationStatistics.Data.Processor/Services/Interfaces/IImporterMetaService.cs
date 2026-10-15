using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterMetaService
{
    Task<SubjectMeta> Import(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSetWriter dataSetWriter
    );

    Task<SubjectMeta> GetSubjectMeta(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSetWriter dataSetWriter
    );
}
