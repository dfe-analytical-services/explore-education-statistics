using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class MongoCsvImporterFactory
    {
        private readonly MongoGeoLevelsCsvImporter mongoGeoLevelsCsvImporter = new MongoGeoLevelsCsvImporter();

        private readonly MongoNationalCharacteristicCsvImporter mongoNationalCharacteristicCsvImporter =
            new MongoNationalCharacteristicCsvImporter();

        private readonly MongoLaCharacteristicCsvImporter mongoLaCharacteristicCsvImporter =
            new MongoLaCharacteristicCsvImporter();

        public IMongoCsvImporter Importer(DataCsvFilename filename)
        {
            switch (filename)
            {
                case DataCsvFilename.absence_geoglevels:
                case DataCsvFilename.exclusion_geoglevels:
                case DataCsvFilename.schpupnum_geoglevels:
                    return mongoGeoLevelsCsvImporter;
                case DataCsvFilename.absence_lacharacteristics:
                case DataCsvFilename.exclusion_lacharacteristics:
                case DataCsvFilename.schpupnum_lacharacteristics:
                    return mongoLaCharacteristicCsvImporter;
                case DataCsvFilename.absence_natcharacteristics:
                case DataCsvFilename.exclusion_natcharacteristics:
                case DataCsvFilename.schpupnum_natcharacteristics:
                    return mongoNationalCharacteristicCsvImporter;      
                default:
                    throw new ArgumentOutOfRangeException(nameof(filename), filename, null);
            }
        }
    }
}