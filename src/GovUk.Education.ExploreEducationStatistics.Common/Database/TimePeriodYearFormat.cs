namespace GovUk.Education.ExploreEducationStatistics.Common.Database
{
    public enum TimePeriodYearFormat
    {
        /**
         * Formats the year without alteration e.g. 2018
         */
        Default,

        /**
         * Formats the year as an academic year e.g. 2018 becomes 2018/19
         */
        Academic,
        
        /**
         * Formats the year as a fiscal year e.g. 2018 becomes 2018-19
         */
        Fiscal
    }
}