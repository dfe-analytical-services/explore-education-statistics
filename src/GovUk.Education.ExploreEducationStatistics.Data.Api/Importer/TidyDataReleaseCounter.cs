using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class TidyDataReleaseCounter
    {
        private readonly GeographicDataService _geographicDataService;
        private readonly LaCharacteristicDataService _laCharacteristicDataService;
        private readonly NationalCharacteristicDataService _nationalCharacteristicDataService;

        public TidyDataReleaseCounter(GeographicDataService geographicDataService,
            LaCharacteristicDataService laCharacteristicDataService,
            NationalCharacteristicDataService nationalCharacteristicDataService)
        {
            _geographicDataService = geographicDataService;
            _laCharacteristicDataService = laCharacteristicDataService;
            _nationalCharacteristicDataService = nationalCharacteristicDataService;
        }

        public int Count(DataCsvFilename filename, Guid publicationId, int releaseId)
        {
            var entityType = filename.GetEntityTypeFromDataFileAttributeOfEnumType(filename.GetType());

            if (entityType == typeof(GeographicData))
            {
                return _geographicDataService.Count(data =>
                    data.PublicationId == publicationId && data.ReleaseId == releaseId);
            }

            if (entityType == typeof(CharacteristicDataLa))
            {
                return _laCharacteristicDataService.Count(data =>
                    data.PublicationId == publicationId && data.ReleaseId == releaseId);
            }

            if (entityType == typeof(CharacteristicDataNational))
            {
                return _nationalCharacteristicDataService.Count(data =>
                    data.PublicationId == publicationId && data.ReleaseId == releaseId);
            }

            throw new ArgumentOutOfRangeException(nameof(filename), filename, null);
        }
    }
}