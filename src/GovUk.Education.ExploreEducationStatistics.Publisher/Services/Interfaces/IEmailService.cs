namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IEmailService
{
    void NotifyEinTilesRequireUpdate(string bauEmail, string bulletsStr);
}
