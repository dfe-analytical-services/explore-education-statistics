using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

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

        public IImporter Importer(ImportFileType importFileType)
        {
            switch (importFileType)
            {
                case ImportFileType.Geographic:
                    return _geographicCsvImporter;
                case ImportFileType.La_Characteristic:
                    return _laCharacteristicCsvImporter;
                case ImportFileType.National_Characteristic:
                    return _nationalCharacteristicCsvImporter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(importFileType), importFileType, null);
            }
        }
    }
}