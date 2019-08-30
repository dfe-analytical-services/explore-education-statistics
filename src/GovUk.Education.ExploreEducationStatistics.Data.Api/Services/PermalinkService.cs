using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private const string ContainerName = "permalinks";

        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public PermalinkService(IDataService<TableBuilderResultViewModel> dataService,
            IFileStorageService fileStorageService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _dataService = dataService;
            _fileStorageService = fileStorageService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public async Task<PermalinkViewModel> GetAsync(Guid id)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, id.ToString());
            var permalink = JsonConvert.DeserializeObject<Permalink>(text);
            return CreatePermalinkViewModel(permalink);
        }

        public async Task<PermalinkViewModel> CreateAsync(TableBuilderQueryContext query)
        {
            var result = _dataService.Query(query);
            var permalink = new Permalink(result, query);
            await _fileStorageService.UploadFromStreamAsync(ContainerName, permalink.Id.ToString(), "application/json",
                JsonConvert.SerializeObject(permalink));
            return CreatePermalinkViewModel(permalink);
        }

        private PermalinkViewModel CreatePermalinkViewModel(Permalink permalink)
        {
            var model = _mapper.Map<PermalinkViewModel>(permalink);
            var subject = _subjectService.Find(permalink.Query.SubjectId, new List<Expression<Func<Subject, object>>>
            {
                s => s.Release
            });
            model.Query.PublicationId = subject.Release.PublicationId;
            return model;
        }
    }
}