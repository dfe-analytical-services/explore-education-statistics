using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Characteristic
    {
        [BsonElement("characteristic_1")] public string Name { get; set; }
        [BsonElement("characteristic_2")] public string Name2 { get; set; }
        [BsonElement("characteristic_desc")] public string Description { get; set; }
    }
}