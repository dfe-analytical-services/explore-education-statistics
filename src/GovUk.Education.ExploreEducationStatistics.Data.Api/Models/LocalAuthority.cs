using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class LocalAuthority
    {
        [BsonElement("la_name")]
        public string Name { get; set; }

        [BsonElement("new_la_code")]
        public string Code { get; set; }

        [BsonElement("old_la_code")]
        public string Old_Code { get; set; }
    }
}