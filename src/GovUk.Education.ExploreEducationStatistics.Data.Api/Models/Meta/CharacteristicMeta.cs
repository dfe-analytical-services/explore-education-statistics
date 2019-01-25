using System;
using MongoDB.Bson;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta
{
    public class CharacteristicMeta : IMeta
    {
        public ObjectId Id { get; set; } 
        public Guid PublicationId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Group { get; set; }
    }
}