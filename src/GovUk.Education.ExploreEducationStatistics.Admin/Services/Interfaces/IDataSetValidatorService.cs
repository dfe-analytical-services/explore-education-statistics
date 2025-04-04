#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetValidatorService
{
    Task<List<ErrorViewModel>> ValidateDataSet(
        Guid releaseVersionId,
        string dataSetTitle,
        List<DataSetFileDto> dataSetFiles,
        File? replacingFile = null);
}
