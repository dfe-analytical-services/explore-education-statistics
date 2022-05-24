#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * Temporary controller for determining the maximum possible size of a table that could be generated
     * by existing datablocks in the service.
     *
     * This figure will be used to establish the limit on the maximum table size of a datablock query that we allow
     * during a validation check before a query is executed.
     *
     * The validation check assumes that all combinations of data have been provided and calculates the maximum
     * possible table size from the query, rejecting the request if it's greater than the set limit.
     *
     * Note that we *don't* execute all datablock queries here and count the results.
     * A table result might be smaller than the possible table that could be generated since all combinations of time
     * period, location and filters may not be provided in the data.
     *
     * If we were to execute all the datablock queries and set a maximum based on the largest table result,
     * several datablock requests may fail if they have a possible table size greater than the largest actual table.
     */
    [ApiController]
    [Authorize]
    public class DataBlockQuerySizeController : ControllerBase
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IFilterItemRepository _filterItemRepository;

        public DataBlockQuerySizeController(ContentDbContext contentDbContext,
            IFilterItemRepository filterItemRepository)
        {
            _contentDbContext = contentDbContext;
            _filterItemRepository = filterItemRepository;
        }

        [HttpGet("api/data-blocks/query-size-report")]
        [Authorize(Roles = RoleNames.BauUser)]
        public async Task<ActionResult<List<DataBlockQuerySizeReport>>> QuerySizeReport()
        {
            var publishedReleaseIds = _contentDbContext.Releases
                .Include(r => r.Publication)
                .ToList()
                .Where(r => r.Publication.IsLatestVersionOfRelease(r.Id))
                .Select(r => r.Id)
                .ToList();

            var publishedDataBlocks = await _contentDbContext.ReleaseContentBlocks
                .Include(rcb => rcb.ContentBlock)
                .Where(rcb => publishedReleaseIds.Contains(rcb.ReleaseId))
                .Select(rcb => rcb.ContentBlock)
                .OfType<DataBlock>()
                .ToListAsync();

            return await publishedDataBlocks
                .ToAsyncEnumerable()
                .SelectAwait(async dataBlock =>
                {
                    var query = dataBlock.Query;

                    var countsOfFilterItemsByFilter = await _filterItemRepository
                        .CountFilterItemsByFilter(dataBlock.Query.Filters);

                    var countOfIndicators = query.Indicators.Count();
                    var countOfLocations = query.LocationIds.Count;
                    var countOfTimePeriods = TimePeriodUtil.Range(query.TimePeriod).Count();

                    var maxTableCellCount = TableBuilderUtils.MaximumTableCellCount(
                        countOfIndicators: countOfIndicators,
                        countOfLocations: countOfLocations,
                        countOfTimePeriods: countOfTimePeriods,
                        countsOfFilterItemsByFilter: countsOfFilterItemsByFilter
                            .Select(pair =>
                            {
                                var (_, count) = pair;
                                return count;
                            }));

                    return new DataBlockQuerySizeReport
                    {
                        DataBlockId = dataBlock.Id,
                        DataBlockName = dataBlock.Name,
                        Filters = countsOfFilterItemsByFilter,
                        Indicators = countOfIndicators,
                        Locations = countOfLocations,
                        TimePeriods = countOfTimePeriods,
                        MaxTableCells = maxTableCellCount
                    };
                })
                .OrderByDescending(result => result.MaxTableCells)
                .ToListAsync();
        }
    }

    public class DataBlockQuerySizeReport
    {
        public Guid DataBlockId { get; set; }
        public string DataBlockName { get; set; } = string.Empty;
        public Dictionary<Guid, int> Filters { get; set; } = new Dictionary<Guid, int>();
        public int Indicators { get; set; }
        public int Locations { get; set; }
        public int TimePeriods { get; set; }
        public int MaxTableCells { get; set; }
    }
}
