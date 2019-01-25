using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MetaService
    {
        private readonly MDatabase _database;

        public MetaService(MDatabase database)
        {
            _database = database;
        }

        public IEnumerable<AttributeMeta> GetAttributeMeta(Guid publicationId)
        {
            return _database.Database.GetCollection<AttributeMeta>("AttributeMeta")
                .Find(meta => meta.PublicationId == publicationId).ToList();
        }

        public IEnumerable<CharacteristicMeta> GetCharacteristicMeta(Guid publicationId)
        {
            return _database.Database.GetCollection<CharacteristicMeta>("CharacteristicMeta")
                .Find(meta => meta.PublicationId == publicationId).ToList();
        }
    }
}