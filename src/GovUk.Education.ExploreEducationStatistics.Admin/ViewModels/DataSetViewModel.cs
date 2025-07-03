using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetViewModel
{
    public List<DataFileInfo> DataFiles { get; init; }

    public List<DataSetUploadViewModel> DataSetUploads { get; init; }
}
