using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class DataBlockService : IDataBlockService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IUserService _userService;
        private readonly ILogger<DataBlockService> _logger;

        public DataBlockService(
            ContentDbContext contentDbContext,
            IBlobStorageService blobStorageService,
            ITableBuilderService tableBuilderService,
            IUserService userService,
            ILogger<DataBlockService> logger)
        {
            _contentDbContext = contentDbContext;
            _blobStorageService = blobStorageService;
            _tableBuilderService = tableBuilderService;
            _userService = userService;
            _logger = logger;
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
                            var path = FileStoragePathUtils.PublicContentDataBlockPath(
                                block.Release.Publication.Slug,
                                block.Release.Slug,
                                block.ContentBlockId
                            );

                            try
                            {
                                return await _blobStorageService.GetDeserializedJson<TableBuilderResultViewModel>(
                                    PublicContentContainerName,
                                    path
                                );
                            }
                            catch (JsonException)
                            {
                                // If there's an error deserializing the blob, we should
                                // assume it's not salvageable and just re-build it.
                                await _blobStorageService.DeleteBlob(PublicContentContainerName, path);
                            }
                            catch (FileNotFoundException)
                            {
                                // Do nothing as the blob just doesn't exist
                            }
                            catch (Exception exception)
                            {
                                _logger.LogWarning(
                                    exception,
                                    $"Unable to fetch data block response from blob storage with path: {path}"
                                );
                            }

                            var query = dataBlock.Query.Clone();
                            query.IncludeGeoJson = dataBlock.Charts.Any(chart => chart.Type == ChartType.Map);

                            return await _tableBuilderService.Query(block.ReleaseId, query)
                                .OnSuccessDo(
                                    result => _blobStorageService.UploadAsJson(PublicContentContainerName, path, result)
                                );
                        }

                        return new NotFoundResult();
                    }
                );
        }
    }
}
