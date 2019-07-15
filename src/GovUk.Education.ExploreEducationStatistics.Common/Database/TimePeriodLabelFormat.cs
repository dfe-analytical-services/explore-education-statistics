namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    /// <summary>
    /// Formats that can be used to label a year and a <c>TimeIdentifier</c>
    /// </summary>
    public enum TimePeriodLabelFormat
    {
        /**
         * Default label formats the TimeIdentifier:
         */

        // Year and label e.g. 2018 July
        Default,

        // Academic or fiscal year and label e.g. 2018/19 Summer Term
        AcademicOrFiscal,

        /**
         * Formats with no label for the TimeIdentifier:
         */

        // Year and no label e.g. 2018
        NoLabel,

        // Academic or fiscal year and no label e.g. 2018/19
        AcademicOrFiscalNoLabel,

        /**
         * Formats with a short label for the TimeIdentifier
         */

        // Year and a short label e.g. 2018 Q1-Q2
        Short,

        // Academic or fiscal year and a short label e.g. 2018/19 Q1-Q2
        AcademicOrFiscalShort
    }
}