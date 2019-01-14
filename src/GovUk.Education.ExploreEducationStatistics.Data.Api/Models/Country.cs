using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Country
    {
//        [BsonElement("country_code")]
        public string Name { get; set; }
        
//        [BsonElement("country_name")]
        public string Code { get; set; }
    }
}