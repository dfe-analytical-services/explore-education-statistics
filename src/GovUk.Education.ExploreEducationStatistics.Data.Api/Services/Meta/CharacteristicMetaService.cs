using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta
{
    public class CharacteristicMetaService : AbstractDataService<CharacteristicMeta>
    {
        public CharacteristicMetaService(ApplicationDbContext context, ILogger<CharacteristicMetaService> logger) :
            base(context, logger)
        {
        }
    }
}