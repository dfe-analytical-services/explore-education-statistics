#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ICacheService _cacheService;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IUserService _userService;

        public DataBlockService(
            ContentDbContext contentDbContext,
            ICacheService cacheService,
            ITableBuilderService tableBuilderService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _cacheService = cacheService;
            _tableBuilderService = tableBuilderService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
            ReleaseContentBlock block)
        {
            // Make sure block is hydrated correctly
            await _contentDbContext.Entry(block)
                .Reference(b => b.ContentBlock)
                .LoadAsync();
            await _contentDbContext.Entry(block)
                .Reference(b => b.Release)
                .LoadAsync();
            await _contentDbContext.Entry(block.Release)
                .Reference(r => r.Publication)
                .LoadAsync();

            return await _userService.CheckCanViewRelease(block.Release)
                .OnSuccess(
                    async _ =>
                    {
                        if (block.ContentBlock is DataBlock dataBlock)
                        {
                            return await _cacheService.GetCachedEntity(
                                PublicContent,
                                new TableBuilderResultCacheKey(block),
                                async () =>
                            {
                                var query = dataBlock.Query.Clone();
                                query.IncludeGeoJson = dataBlock.Charts.Any(chart => chart.Type == ChartType.Map);

                                return await _tableBuilderService.Query(block.ReleaseId, query);
                            });
                        }

                        return new NotFoundResult();
                    }
                );
        }
    }
}
