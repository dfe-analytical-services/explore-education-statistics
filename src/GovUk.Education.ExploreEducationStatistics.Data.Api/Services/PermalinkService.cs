using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        public PermalinkService()
        {
        }

        public Permalink Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Permalink Create()
        {
            return new Permalink()
            {
                Id = new Guid()
            };
        }
    }
}