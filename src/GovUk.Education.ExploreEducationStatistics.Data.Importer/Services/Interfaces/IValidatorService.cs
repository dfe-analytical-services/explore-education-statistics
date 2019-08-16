namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IValidatorService
    {
        void ValidateMetaHeader(long subjectId, string header);

        void ValidateMetaRow(long subjectId, string row, int rowNumber, int numExpectedColumns);
    }
}