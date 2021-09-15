#nullable enable
using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MapperUtils
    {
        public static IMapper AdminMapper()
        {
            var serviceLookupByType = new Dictionary<Type, object>
            {
                {
                    typeof(IMyPublicationPermissionSetPropertyResolver),
                    new Mock<IMyPublicationPermissionSetPropertyResolver>().Object
                },
                {
                    typeof(IMyReleasePermissionSetPropertyResolver),
                    new Mock<IMyReleasePermissionSetPropertyResolver>().Object
                },
                {
                    typeof(IMyPublicationMethodologyPermissionsPropertyResolver),
                    new Mock<IMyPublicationMethodologyPermissionsPropertyResolver>().Object
                },
                {
                    typeof(IMyMethodologyPermissionSetPropertyResolver),
                    new Mock<IMyMethodologyPermissionSetPropertyResolver>().Object
                }
            };

            object ServiceLocator(Type serviceType) => serviceLookupByType[serviceType];
            return Common.Services.MapperUtils.MapperForProfile<MappingProfiles>(ServiceLocator);
        }
    }
}
