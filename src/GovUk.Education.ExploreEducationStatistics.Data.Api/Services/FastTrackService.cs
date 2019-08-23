using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private const string ContainerName = "fasttrack";

        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public FastTrackService(
            IDataService<TableBuilderResultViewModel> dataService,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _dataService = dataService;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<FastTrackViewModel> GetAsync(Guid fastTrackId)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, fastTrackId.ToString());
            var fastTrack = JsonConvert.DeserializeObject<FastTrack>(text);
            var data = _dataService.Query(fastTrack.Query);
            return BuildViewModel(fastTrack, data);
        }

        private FastTrackViewModel BuildViewModel(FastTrack fastTrack, TableBuilderResultViewModel result)
        {
            var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
            viewModel.FullTable = result;
            return viewModel;
        }
    }
}