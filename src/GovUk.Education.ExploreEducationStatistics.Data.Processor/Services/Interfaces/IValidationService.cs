using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidationService
    {
        void Validate(ICollector<ImportMessage> collector, ImportMessage message);
    }
}