using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class TidyDataPersisterFactory
    {
        private readonly GeographicTidyDataPersister _geographicTidyPersister;
        private readonly LaCharacteristicTidyDataPersister _laCharacteristicTidyDtaPersister;
        private readonly NationalCharacteristicTidyDataPersister _nationalCharacteristicTidyDataPersister;

        public TidyDataPersisterFactory(GeographicTidyDataPersister geographicTidyPersister,
            LaCharacteristicTidyDataPersister laCharacteristicTidyDtaPersister,
            NationalCharacteristicTidyDataPersister nationalCharacteristicTidyDataPersister)
        {
            _geographicTidyPersister = geographicTidyPersister;
            _laCharacteristicTidyDtaPersister = laCharacteristicTidyDtaPersister;
            _nationalCharacteristicTidyDataPersister = nationalCharacteristicTidyDataPersister;
        }

        public ITidyDataPersister Persister(DataCsvFilename filename)
        {
            switch (filename)
            {
                case DataCsvFilename.absence_geoglevels:
                case DataCsvFilename.exclusion_geoglevels:
                case DataCsvFilename.schpupnum_geoglevels:
                    return _geographicTidyPersister;

                case DataCsvFilename.absence_lacharacteristics:
                case DataCsvFilename.exclusion_lacharacteristics:
                case DataCsvFilename.schpupnum_lacharacteristics:
                    return _laCharacteristicTidyDtaPersister;

                case DataCsvFilename.absence_natcharacteristics:
                case DataCsvFilename.exclusion_natcharacteristics:
                case DataCsvFilename.schpupnum_natcharacteristics:
                    return _nationalCharacteristicTidyDataPersister;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filename), filename, null);
            }
        }
    }
}