using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TidyData
    {
        public ObjectId Id { get; set; }
        
        // potentially optional
        [BsonElement("term")]
        public string Term { get; set; }
        
        [BsonElement("year")]
        public int Year { get; set; }
        
        [BsonElement("level")]
        public string Level { get; set; }
        
        public Country Country { get; set; }
        
        public Region Region { get; set; }
        
        public LocalAuthority LocalAuthority { get; set; }

        [BsonElement("estab")]
        public string Estab { get; set; }
        
        [BsonElement("laestab")]
        public string LaEstab { get; set; }
        
        [BsonElement("school_type")]
        public string SchoolType { get; set; }
    }
}