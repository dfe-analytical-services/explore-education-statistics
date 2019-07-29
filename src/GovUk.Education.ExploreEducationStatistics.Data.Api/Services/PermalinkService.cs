namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
    using Microsoft.Azure.Cosmos.Table;
    using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
    using Newtonsoft.Json;

    public class PermalinkService : IPermalinkService
    {
        private const string PermalinkTableName = "permalink";
        private readonly ITableStorageService _tableStorageService;

        private readonly IDataService<ResultWithMetaViewModel> _dataService;


        public PermalinkService(ITableStorageService tableStorageService, IDataService<ResultWithMetaViewModel> dataService)
        {
            _tableStorageService = tableStorageService;
            _dataService = dataService;
        }

        public async Task<PermalinkViewModel> GetAsync(Guid id)
        {
            var table = await _tableStorageService.GetTableAsync(PermalinkTableName);

            var retrieveOperation = TableOperation.Retrieve<PermalinkEntity>("the-publication-id", id.ToString());
            var result = await table.ExecuteAsync(retrieveOperation);

            if (result.Result is PermalinkEntity permalink)
            {
                var model = new PermalinkViewModel()
                {
                    Id = permalink.RowKey,
                    Title = permalink.Title,
                    Data = JsonConvert.DeserializeObject<ResultWithMetaViewModel>(permalink.Data)
                };

                return model;
            }

            return null;
        }

        public async Task<PermalinkViewModel> CreateAsync(ObservationQueryContext tableQuery)
        {
            var table = await _tableStorageService.GetTableAsync(PermalinkTableName);

            var data = await _dataService.QueryAsync(tableQuery);

            var permalink = new PermalinkEntity()
            {
                // passing the title generated from the frontend could be exploited so we'll need to generate something
                Title = "Auto-generated table title",
                Data = JsonConvert.SerializeObject(data),
                Query = JsonConvert.SerializeObject(tableQuery)
            };

            var insertOperation = TableOperation.Insert(permalink);
            var result = await table.ExecuteAsync(insertOperation);

            if (result.Result is PermalinkEntity insertedPermalink)
            {
                var model = new PermalinkViewModel()
                {
                    Id = insertedPermalink.RowKey,
                    Title = insertedPermalink.Title,
                    Data = JsonConvert.DeserializeObject<ResultWithMetaViewModel>(insertedPermalink.Data)
                };

                return model;
            }

            throw new Microsoft.WindowsAzure.Storage.StorageException();
        }
    }
}