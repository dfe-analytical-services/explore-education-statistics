using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public enum DataCsvFilename
    {
        [DataFile(ImportFileType.Geographic)]
        absence_geoglevels,

        [DataFile(ImportFileType.Geographic)]
        exclusion_geoglevels,

        [DataFile(ImportFileType.Geographic)]
        schpupnum_geoglevels,

        [DataFile(ImportFileType.La_Characteristic)]
        absence_lacharacteristics,

        [DataFile(ImportFileType.La_Characteristic)]
        exclusion_lacharacteristics,

        [DataFile(ImportFileType.La_Characteristic)]
        schpupnum_lacharacteristics,

        [DataFile(ImportFileType.National_Characteristic)]
        absence_natcharacteristics,

        [DataFile(ImportFileType.National_Characteristic)]
        exclusion_natcharacteristics,

        [DataFile(ImportFileType.National_Characteristic)]
        schpupnum_natcharacteristics
    }
}