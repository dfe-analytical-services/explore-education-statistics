using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
{
    public class CsvImporterFactory
    {
        private readonly GeographicImporter _geographicCsvImporter;
        private readonly NationalCharacteristicImporter _nationalCharacteristicCsvImporter;
        private readonly LaCharacteristicImporter _laCharacteristicCsvImporter;

        public CsvImporterFactory(GeographicImporter geographicCsvImporter,
            NationalCharacteristicImporter nationalCharacteristicCsvImporter,
            LaCharacteristicImporter laCharacteristicCsvImporter)
        {
            _geographicCsvImporter = geographicCsvImporter;
            _nationalCharacteristicCsvImporter = nationalCharacteristicCsvImporter;
            _laCharacteristicCsvImporter = laCharacteristicCsvImporter;
        }

        public IImporter Importer(DataCsvFilename filename)
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