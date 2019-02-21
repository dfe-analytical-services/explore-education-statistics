using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public interface ITidyDataPersister
    {
        int Count(Guid publicationId);
    }
}