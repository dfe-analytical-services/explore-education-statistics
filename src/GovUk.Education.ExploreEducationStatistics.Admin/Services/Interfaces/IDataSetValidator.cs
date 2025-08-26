#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetValidator
{
    Task<Either<List<ErrorViewModel>, DataSet>> ValidateDataSet(DataSetDto dataSet);

    Task<Either<List<ErrorViewModel>, DataSetIndex>> ValidateBulkDataZipIndexFile(
        Guid releaseVersionId,
        FileDto indexFile,
        List<FileDto> dataSetFiles);
}
