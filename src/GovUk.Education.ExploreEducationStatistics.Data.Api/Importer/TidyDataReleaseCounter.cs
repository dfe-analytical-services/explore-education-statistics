using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class TidyDataReleaseCounter
    {
        private readonly IGeographicDataService _geographicDataService;
        private readonly ILaCharacteristicDataService _laCharacteristicDataService;
        private readonly INationalCharacteristicDataService _nationalCharacteristicDataService;

        public TidyDataReleaseCounter(IGeographicDataService geographicDataService,
            ILaCharacteristicDataService laCharacteristicDataService,
            INationalCharacteristicDataService nationalCharacteristicDataService)
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