#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IUserService _userService;

        public DataBlockService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
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

            return await _userService
                .CheckCanViewRelease(block.Release)
                .OnSuccess(_ => GetTableResult(block));
        }

        [BlobCache(typeof(DataBlockTableResultCacheKey))]
        private async Task<Either<ActionResult, TableBuilderResultViewModel>> GetTableResult(
            ReleaseContentBlock block)
        {
            if (block.ContentBlock is DataBlock dataBlock)
            {
                var query = dataBlock.Query.Clone();
                query.IncludeGeoJson = dataBlock.Charts.Any(chart => chart.Type == ChartType.Map);

                return await _tableBuilderService.Query(block.ReleaseId, query);
            }

            return new NotFoundResult();
        }
    }
}