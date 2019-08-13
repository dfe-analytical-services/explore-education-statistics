using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private const string ContainerName = "permalinks";

        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public PermalinkService(IDataService<TableBuilderResultViewModel> dataService,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _dataService = dataService;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<PermalinkViewModel> GetAsync(Guid id)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, id.ToString());
            var permalink = JsonConvert.DeserializeObject<Permalink>(text);
            return _mapper.Map<PermalinkViewModel>(permalink);
        }

        public async Task<PermalinkViewModel> CreateAsync(PermalinkQueryContext query)
        {
            var result = _dataService.Query(query);
            var permalink = new Permalink(result, query);
            await _fileStorageService.UploadFromStreamAsync(ContainerName, permalink.Id.ToString(), "application/json",
                JsonConvert.SerializeObject(permalink));
            return _mapper.Map<PermalinkViewModel>(permalink);
        }
    }
}