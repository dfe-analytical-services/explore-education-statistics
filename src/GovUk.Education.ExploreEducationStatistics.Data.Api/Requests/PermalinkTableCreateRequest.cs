#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public class PermalinkTableCreateRequest
{
    public TableBuilderResultViewModel FullTable { get; set; }

    public TableBuilderConfiguration Configuration { get; set; }
}