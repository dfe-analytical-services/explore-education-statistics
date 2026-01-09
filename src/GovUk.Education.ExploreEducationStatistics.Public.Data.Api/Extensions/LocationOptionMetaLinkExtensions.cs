using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class LocationOptionMetaLinkExtensions
{
    public static LocationOptionViewModel ToViewModel(this LocationOptionMetaLink link)
    {
        return link.Option.ToViewModel(link.PublicId);
    }
}
