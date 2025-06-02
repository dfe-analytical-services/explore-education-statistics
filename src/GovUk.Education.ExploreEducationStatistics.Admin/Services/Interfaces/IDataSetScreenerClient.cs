using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetScreenerClient
{
    Task<List<DataSetUploadResultViewModel>> ScreenDataSet(
        List<DataSetUploadResultViewModel> dataSets);
}
