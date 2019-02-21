using System;

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
            switch (filename)
            {
                case DataCsvFilename.absence_geoglevels:
                case DataCsvFilename.exclusion_geoglevels:
                case DataCsvFilename.schpupnum_geoglevels:
                    return _geographicCsvImporter;
                case DataCsvFilename.absence_lacharacteristics:
                case DataCsvFilename.exclusion_lacharacteristics:
                case DataCsvFilename.schpupnum_lacharacteristics:
                    return _laCharacteristicCsvImporter;
                case DataCsvFilename.absence_natcharacteristics:
                case DataCsvFilename.exclusion_natcharacteristics:
                case DataCsvFilename.schpupnum_natcharacteristics:
                    return _nationalCharacteristicCsvImporter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filename), filename, null);
            }
        }
    }
}