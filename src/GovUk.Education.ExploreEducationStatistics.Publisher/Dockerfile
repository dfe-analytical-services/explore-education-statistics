FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env

COPY GovUk.Education.ExploreEducationStatistics.Common/GovUk.Education.ExploreEducationStatistics.Common.csproj GovUk.Education.ExploreEducationStatistics.Common/
COPY GovUk.Education.ExploreEducationStatistics.Content.Model/GovUk.Education.ExploreEducationStatistics.Content.Model.csproj GovUk.Education.ExploreEducationStatistics.Content.Model/
COPY GovUk.Education.ExploreEducationStatistics.Content.Services/GovUk.Education.ExploreEducationStatistics.Content.Services.csproj GovUk.Education.ExploreEducationStatistics.Content.Services/
COPY GovUk.Education.ExploreEducationStatistics.Data.Model/GovUk.Education.ExploreEducationStatistics.Data.Model.csproj GovUk.Education.ExploreEducationStatistics.Data.Model/
COPY GovUk.Education.ExploreEducationStatistics.Data.Services/GovUk.Education.ExploreEducationStatistics.Data.Services.csproj GovUk.Education.ExploreEducationStatistics.Data.Services/
COPY GovUk.Education.ExploreEducationStatistics.Notifier.Model/GovUk.Education.ExploreEducationStatistics.Notifier.Model.csproj GovUk.Education.ExploreEducationStatistics.Notifier.Model/
COPY GovUk.Education.ExploreEducationStatistics.Publisher.Model/GovUk.Education.ExploreEducationStatistics.Publisher.Model.csproj GovUk.Education.ExploreEducationStatistics.Publisher.Model/

COPY GovUk.Education.ExploreEducationStatistics.Publisher/GovUk.Education.ExploreEducationStatistics.Publisher.csproj GovUk.Education.ExploreEducationStatistics.Publisher/

RUN cd /GovUk.Education.ExploreEducationStatistics.Publisher && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]