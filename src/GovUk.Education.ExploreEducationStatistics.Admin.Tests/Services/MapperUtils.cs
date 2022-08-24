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
                    typeof(IMyPublicationPermissionsResolver),
                    new Mock<IMyPublicationPermissionsResolver>().Object
                },
                {
                    typeof(IMyPublicationMethodologyVersionPermissionsResolver),
                    new Mock<IMyPublicationMethodologyVersionPermissionsResolver>().Object
                },
                {
                    typeof(IMyMethodologyVersionPermissionsResolver),
                    new Mock<IMyMethodologyVersionPermissionsResolver>().Object
                }
            };

            object ServiceLocator(Type serviceType) => serviceLookupByType[serviceType];
            return Common.Services.MapperUtils.MapperForProfile<MappingProfiles>(ServiceLocator);
        }
    }
}
