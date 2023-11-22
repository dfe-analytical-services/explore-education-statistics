#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IDataGuidanceDataSetService _dataGuidanceDataSetService;
        private readonly IUserService _userService;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;

        public DataGuidanceService(ContentDbContext contentDbContext,
            IDataGuidanceDataSetService dataGuidanceDataSetService,
            IUserService userService,
            IReleaseDataFileRepository releaseDataFileRepository)
        {
            _contentDbContext = contentDbContext;
            _dataGuidanceDataSetService = dataGuidanceDataSetService;
            _userService = userService;
            _releaseDataFileRepository = releaseDataFileRepository;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(Guid releaseId,
            CancellationToken cancellationToken = default)
        {
            return await _contentDbContext.Releases
                .FirstOrNotFoundAsync(release => release.Id == releaseId, cancellationToken)
                .OnSuccess(release => _userService.CheckCanViewRelease(release))
                .OnSuccessCombineWith(_ =>
                    _dataGuidanceDataSetService.ListDataSets(releaseId, cancellationToken: cancellationToken))
                .OnSuccess(releaseAndDataSets => BuildViewModel(releaseAndDataSets.Item1, releaseAndDataSets.Item2));
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> UpdateDataGuidance(Guid releaseId,
            DataGuidanceUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _contentDbContext.Releases
                .FirstOrNotFoundAsync(release => release.Id == releaseId, cancellationToken)
                .OnSuccess(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccessDo(release => UpdateDataGuidance(release, request, cancellationToken))
                .OnSuccessCombineWith(_ =>
                    _dataGuidanceDataSetService.ListDataSets(releaseId, cancellationToken: cancellationToken))
                .OnSuccess(releaseAndDataSets => BuildViewModel(releaseAndDataSets.Item1, releaseAndDataSets.Item2));
        }

        public async Task<Either<ActionResult, Unit>> Validate(Guid releaseId,
            CancellationToken cancellationToken = default)
        {
            return await _contentDbContext.Releases
                .FirstOrNotFoundAsync(release => release.Id == releaseId, cancellationToken)
                .OnSuccess(async release =>
                {
                    if (await _releaseDataFileRepository.HasAnyDataFiles(release.Id))
                    {
                        if (string.IsNullOrWhiteSpace(release.DataGuidance))
                        {
                            return ValidationResult(PublicDataGuidanceRequired);
                        }

                        // TODO EES-4661 Inline this validation into this service as soon as we
                        // only need to check release files rather than subjects.
                        return await _dataGuidanceDataSetService.Validate(releaseId, cancellationToken);
                    }

                    return Unit.Instance;
                });
        }

        private async Task<Either<ActionResult, Unit>> UpdateDataGuidance(
            Release release,
            DataGuidanceUpdateRequest request,
            CancellationToken cancellationToken)
        {
            _contentDbContext.Update(release);
            release.DataGuidance = request.Content;

            var allDataFileIds = (await _releaseDataFileRepository.ListDataFiles(release.Id))
                .Select(file => file.Id);

            var updateRequestsByFileId = request.DataSets.ToDictionary(dataSet => dataSet.FileId);
            var dataFileIds = updateRequestsByFileId.Keys.ToList();

            if (!allDataFileIds.ContainsAll(dataFileIds))
            {
                return ValidationResult(DataGuidanceDataSetNotAttachedToRelease);
            }

            var releaseFiles = await _contentDbContext
                .ReleaseFiles
                .Include(releaseFile => releaseFile.File)
                .Where(releaseFile => releaseFile.ReleaseId == release.Id
                                      && dataFileIds.Contains(releaseFile.FileId))
                .ToListAsync(cancellationToken);

            releaseFiles.ForEach(releaseFile =>
            {
                var content = updateRequestsByFileId[releaseFile.FileId].Content;

                _contentDbContext.Update(releaseFile);
                releaseFile.Summary = content;
            });

            await _contentDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Instance;
        }

        private static DataGuidanceViewModel BuildViewModel(Release release,
            List<DataGuidanceDataSetViewModel> dataSets)
        {
            return new DataGuidanceViewModel
            {
                Id = release.Id,
                Content = release.DataGuidance ?? "",
                DataSets = dataSets
            };
        }
    }
}
