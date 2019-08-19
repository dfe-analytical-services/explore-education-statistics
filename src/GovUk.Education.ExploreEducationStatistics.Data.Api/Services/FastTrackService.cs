using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private const string ContainerName = "fasttrack";

        private readonly IDataService<TableBuilderResultViewModel> _dataService;
        private readonly IFileStorageService _fileStorageService;

        public FastTrackService(IDataService<TableBuilderResultViewModel> dataService, IFileStorageService fileStorageService)
        {
            _dataService = dataService;
            _fileStorageService = fileStorageService;
        }
        public async Task<FastTrackViewModel> GetAsync(Guid fastTrackId)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, fastTrackId.ToString());

            // TODO: Define the "FastTrack" Model (will want to be in common)
            
            // TODO: Map Create view model 
            
            // TODO: Query data and add to view model
            
            throw new NotImplementedException();
        }
    }
}