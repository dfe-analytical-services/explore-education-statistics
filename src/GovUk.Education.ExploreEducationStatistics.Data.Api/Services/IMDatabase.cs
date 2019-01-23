using MongoDB.Driver;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public interface IMDatabase<T>
    {
        IMongoDatabase Database { get; }
        IMongoCollection<T> Collection(string collectionName);
    }
}