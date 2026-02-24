namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface IEmailService
{
    void SendEmail(string email, string templateId, Dictionary<string, dynamic> values);
}
