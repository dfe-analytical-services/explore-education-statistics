using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class School
    {
        [BsonElement("estab")] public string Estab { get; set; }
        [BsonElement("laestab")] public string LaEstab { get; set; }
        [BsonElement("academy_type")] public string AcademyType { get; set; }
        [BsonElement("academy_open_date")] public string AcademyOpenDate { get; set; }
    }
}