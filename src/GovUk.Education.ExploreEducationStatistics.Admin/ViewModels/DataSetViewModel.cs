using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class DataSetViewModel
{
    public List<DataFileInfo> DataFiles { get; set; }

    public List<DataSetUploadViewModel> DataSetUploads { get; set; }
}
