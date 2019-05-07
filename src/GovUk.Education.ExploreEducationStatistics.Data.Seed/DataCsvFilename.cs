namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public enum DataCsvFilename
    {
        [DataFile(DataCsvMetaFilename.meta_absence_by_characteristic)]
        absence_by_characteristic,

        [DataFile(DataCsvMetaFilename.meta_absence_by_geographic_level)]
        absence_by_geographic_level,

        [DataFile(DataCsvMetaFilename.meta_absence_by_term)]
        absence_by_term,

        [DataFile(DataCsvMetaFilename.meta_absence_for_four_year_olds)]
        absence_for_four_year_olds,

        [DataFile(DataCsvMetaFilename.meta_absence_in_prus)]
        absence_in_prus,

        [DataFile(DataCsvMetaFilename.meta_absence_rate_percent_bands)]
        absence_rate_percent_bands,

        [DataFile(DataCsvMetaFilename.meta_absence_number_missing_at_least_one_session_by_reason)]
        absence_number_missing_at_least_one_session_by_reason
    }
}