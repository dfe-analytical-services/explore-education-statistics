using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IImporterMetaService
{
    (List<Filter> Filters, List<Indicator> Indicators) ReadMetaCsv(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        Guid subjectId
    );
}
