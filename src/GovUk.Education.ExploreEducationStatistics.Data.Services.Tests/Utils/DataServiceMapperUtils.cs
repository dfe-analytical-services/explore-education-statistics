using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils
{
    public class DataServiceMapperUtils
    {
        public static IMapper DataServiceMapper()
        {
            var serviceLookupByType = new Dictionary<Type, object>
            {
            };

            object ServiceLocator(Type serviceType) => serviceLookupByType[serviceType];
            return Common.Services.MapperUtils.MapperForProfile<DataServiceMappingProfiles>(ServiceLocator);
        }
    }
}