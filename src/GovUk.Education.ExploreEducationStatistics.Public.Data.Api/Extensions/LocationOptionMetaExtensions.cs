using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class LocationOptionMetaExtensions
{
    public static LocationOptionViewModel ToViewModel(this LocationOptionMeta optionMeta, string publicId)
    {
        return optionMeta switch
        {
            LocationCodedOptionMeta codedOption => new LocationCodedOptionViewModel
            {
                Id = publicId,
                Label = codedOption.Label,
                Code = codedOption.Code,
            },
            LocationLocalAuthorityOptionMeta localAuthorityOption => new LocationLocalAuthorityOptionViewModel
            {
                Id = publicId,
                Label = localAuthorityOption.Label,
                Code = localAuthorityOption.Code,
                OldCode = localAuthorityOption.OldCode,
            },
            LocationProviderOptionMeta providerOption => new LocationProviderOptionViewModel
            {
                Id = publicId,
                Label = providerOption.Label,
                Ukprn = providerOption.Ukprn,
            },
            LocationRscRegionOptionMeta rscRegionOption => new LocationRscRegionOptionViewModel
            {
                Id = publicId,
                Label = rscRegionOption.Label,
            },
            LocationSchoolOptionMeta schoolOption => new LocationSchoolOptionViewModel
            {
                Id = publicId,
                Label = schoolOption.Label,
                Urn = schoolOption.Urn,
                LaEstab = schoolOption.LaEstab,
            },
            _ => throw new NotImplementedException(),
        };
    }
}
