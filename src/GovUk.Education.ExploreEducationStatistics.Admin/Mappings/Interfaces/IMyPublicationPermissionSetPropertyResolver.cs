using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces
{
    public interface IMyPublicationPermissionSetPropertyResolver
        : IValueResolver<Publication, MyPublicationViewModel, MyPublicationViewModel.PermissionsSet> 
    {
        
    }
}