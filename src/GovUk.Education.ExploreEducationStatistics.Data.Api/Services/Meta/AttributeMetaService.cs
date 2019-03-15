using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta
{
    public class AttributeMetaService : AbstractDataService<AttributeMeta>
    {
        public AttributeMetaService(ApplicationDbContext context, ILogger<AttributeMetaService> logger) :
            base(context, logger)
        {
        }
    }
}