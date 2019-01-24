using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    [BsonKnownTypes(typeof(GeographicData), typeof(CharacteristicDataLa),
        typeof(CharacteristicDataNational))]
    public abstract class TidyData : ITidyData
    {
        protected TidyData()
        {
        }

        protected TidyData(Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            string term,
            int year,
            string level,
            Country country,
            string schoolType,
            Dictionary<string, string> attributes)
        {
            PublicationId = publicationId;
            ReleaseId = releaseId;
            ReleaseDate = releaseDate;
            Term = term;
            Year = year;
            Level = level;
            Country = country;
            SchoolType = schoolType;
            Attributes = attributes;
        }

        public ObjectId Id { get; set; }
        public Guid PublicationId { get; set; }
        public Guid ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }
        [BsonElement("term")] public string Term { get; set; }
        [BsonElement("year")] public int Year { get; set; }
        [BsonElement("level")] public string Level { get; set; }
        public Country Country { get; set; }
        [BsonElement("school_type")] public string SchoolType { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}