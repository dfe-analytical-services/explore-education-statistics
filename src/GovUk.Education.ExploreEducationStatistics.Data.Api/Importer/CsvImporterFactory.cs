using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class CsvImporterFactory
    {
        private readonly GeographicCsvImporter _geographicCsvImporter = new GeographicCsvImporter();

        private readonly NationalCharacteristicCsvImporter _nationalCharacteristicCsvImporter =
            new NationalCharacteristicCsvImporter();

        private readonly LaCharacteristicCsvImporter _laCharacteristicCsvImporter =
            new LaCharacteristicCsvImporter();

        public ICsvImporter Importer(DataCsvFilename filename)
        {
            var entityType = filename.GetDataTypeFromDataFileAttributeOfEnumType(filename.GetType());

            if (entityType == typeof(GeographicData))
            {
                return _geographicCsvImporter;
            }

            if (entityType == typeof(CharacteristicDataLa))
            {
                return _laCharacteristicCsvImporter;
            }

            if (entityType == typeof(CharacteristicDataNational))
            {
                return _nationalCharacteristicCsvImporter;
            }

            throw new ArgumentOutOfRangeException(nameof(filename), filename, null);
        }
    }
}