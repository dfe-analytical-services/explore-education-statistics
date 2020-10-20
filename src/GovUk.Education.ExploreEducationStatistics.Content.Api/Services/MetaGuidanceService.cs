﻿using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;

        public MetaGuidanceService(IBlobStorageService blobStorageService,
            IMetaGuidanceSubjectService metaGuidanceSubjectService)
        {
            _blobStorageService = blobStorageService;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(string releasePath)
        {
            string text;
            try
            {
                text = await _blobStorageService.DownloadBlobText(
                    PublicContentContainerName,
                    releasePath
                );
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new JsonException(
                    $"Found empty file when trying to deserialize Release JSON for path {releasePath}");
            }

            var release = JsonConvert.DeserializeObject<CachedReleaseViewModel>(
                text,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );

            return await _metaGuidanceSubjectService.GetSubjects(release.Id)
                .OnSuccess(subjects =>
                    new MetaGuidanceViewModel
                    {
                        Id = release.Id,
                        Content = release.MetaGuidance,
                        Subjects = subjects
                    });
        }
    }
}