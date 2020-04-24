using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private const string ContainerName = "permalinks";

        private readonly ITableBuilderService _tableBuilderService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public PermalinkService(ITableBuilderService tableBuilderService,
            IFileStorageService fileStorageService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _fileStorageService = fileStorageService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> GetAsync(Guid id)
        {
            try
            {
                var text = await _fileStorageService.DownloadTextAsync(ContainerName, id.ToString());
                var permalink = JsonConvert.DeserializeObject<Permalink>(text);
                return BuildViewModel(permalink);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> CreateAsync(TableBuilderQueryContext query)
        {
            return await _tableBuilderService.Query(query).OnSuccess(async result =>
            {
                var permalink = new Permalink(result, query);
                await _fileStorageService.UploadFromStreamAsync(ContainerName, permalink.Id.ToString(),
                    "application/json",
                    JsonConvert.SerializeObject(permalink));
                return BuildViewModel(permalink);
            });
        }

        private PermalinkViewModel BuildViewModel(Permalink permalink)
        {
            var viewModel = _mapper.Map<PermalinkViewModel>(permalink);
            viewModel.Query.PublicationId =
                _subjectService.GetPublicationForSubjectAsync(permalink.Query.SubjectId).Result.Id;
            return viewModel;
        }
    }
}