using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private readonly CloudBlobClient _client;
        private const string _containerName = "permalinks";

        private readonly IDataService<ResultWithMetaViewModel> _dataService;


        public PermalinkService(IConfiguration config, IDataService<ResultWithMetaViewModel> dataService)
        {
            _dataService = dataService;

            var storageAccount = CloudStorageAccount.Parse(config.GetConnectionString("PublicStorage"));
            _client = storageAccount.CreateCloudBlobClient();
        }

        public async Task<PermalinkViewModel> GetAsync(Guid id)
        {
            var container = _client.GetContainerReference(_containerName);
            var blob = container.GetBlockBlobReference(id.ToString());

            if (!blob.Exists())    
            {
                return null;
            }

            var permalink = JsonConvert.DeserializeObject<PermalinkEntity>(blob.DownloadText());
            
            var model = new PermalinkViewModel
            {
                Id = permalink.Id.ToString(),
                Title = permalink.Title,
                Created = permalink.Created,
                Data = permalink.Data
            };

            return model;
        }

        public async Task<PermalinkViewModel> CreateAsync(ObservationQueryContext tableQuery)
        {
            // get the data
            var data = await _dataService.QueryAsync(tableQuery);

            var container = _client.GetContainerReference(_containerName);
            await container.CreateIfNotExistsAsync();

            var permalink = new PermalinkEntity()
            {
                // TODO: passing the title generated from the frontend could be exploited so we'll need to generate something
                Title = "Auto-generated table title",
                Data = data,
                Query = tableQuery
            };

            var blob = container.GetBlockBlobReference(permalink.Id.ToString());
            blob.Properties.ContentType = "application/json";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(permalink))))
            {
                await blob.UploadFromStreamAsync(stream);
            }

            if (!blob.Exists()) return null;

            var model = new PermalinkViewModel()
            {
                Id = permalink.Id.ToString(),
                Title = permalink.Title,
                Created = permalink.Created,
            };

            return model;
        }
    }
}