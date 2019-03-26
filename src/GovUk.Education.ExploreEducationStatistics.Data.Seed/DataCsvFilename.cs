using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public enum DataCsvFilename
    {
        [DataFile(typeof(GeographicData))]
        absence_geoglevels,
        [DataFile(typeof(GeographicData))]
        exclusion_geoglevels,
        [DataFile(typeof(GeographicData))]
        schpupnum_geoglevels,
        
        [DataFile(typeof(CharacteristicDataLa))]
        absence_lacharacteristics,
        [DataFile(typeof(CharacteristicDataLa))]
        exclusion_lacharacteristics,
        [DataFile(typeof(CharacteristicDataLa))]
        schpupnum_lacharacteristics,
        
        [DataFile(typeof(CharacteristicDataNational))]
        absence_natcharacteristics,
        [DataFile(typeof(CharacteristicDataNational))]
        exclusion_natcharacteristics,
        [DataFile(typeof(CharacteristicDataNational))]
        schpupnum_natcharacteristics
    }
}