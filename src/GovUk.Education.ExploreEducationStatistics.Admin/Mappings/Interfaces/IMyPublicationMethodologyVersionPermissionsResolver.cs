#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces
{
    public interface IMyPublicationMethodologyVersionPermissionsResolver : IValueResolver<
        PublicationMethodology,
        MyPublicationMethodologyVersionViewModel,
        MyPublicationMethodologyVersionViewModel.PermissionsSet>
    {
    }
}
