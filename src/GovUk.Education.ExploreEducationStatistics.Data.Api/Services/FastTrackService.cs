using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private const string ContainerName = "fasttrack";

        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public FastTrackService(
            IDataService<TableBuilderResultViewModel> dataService,
            IFileStorageService fileStorageService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _dataService = dataService;
            _fileStorageService = fileStorageService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public async Task<FastTrackViewModel> GetAsync(Guid fastTrackId)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, fastTrackId.ToString());
            var fastTrack = JsonConvert.DeserializeObject<FastTrack>(text);
            return BuildViewModel(fastTrack);
        }

        private FastTrackViewModel BuildViewModel(FastTrack fastTrack)
        {
            var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
            var subject = _subjectService.Find(fastTrack.Query.SubjectId, new List<Expression<Func<Subject, object>>>
            {
                s => s.Release
            });
            var result = _dataService.Query(fastTrack.Query);
            viewModel.FullTable = result;
            viewModel.Query.PublicationId = subject.Release.PublicationId;
            return viewModel;
        }
    }
}