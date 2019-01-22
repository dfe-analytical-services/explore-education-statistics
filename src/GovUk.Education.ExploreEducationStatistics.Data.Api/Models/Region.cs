using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Region
    {
        [BsonElement("region_code")] public string Code { get; set; }
        [BsonElement("region_name")] public string Name { get; set; }
    }
}