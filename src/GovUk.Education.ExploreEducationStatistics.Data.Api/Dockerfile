FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY GovUk.Education.ExploreEducationStatistics.Common/GovUk.Education.ExploreEducationStatistics.Common.csproj GovUk.Education.ExploreEducationStatistics.Common/
COPY GovUk.Education.ExploreEducationStatistics.Data.Model/GovUk.Education.ExploreEducationStatistics.Data.Model.csproj GovUk.Education.ExploreEducationStatistics.Data.Model/
COPY GovUk.Education.ExploreEducationStatistics.Data.Services/GovUk.Education.ExploreEducationStatistics.Data.Services.csproj GovUk.Education.ExploreEducationStatistics.Data.Services/
COPY GovUk.Education.ExploreEducationStatistics.Content.Model/GovUk.Education.ExploreEducationStatistics.Content.Model.csproj GovUk.Education.ExploreEducationStatistics.Content.Model/
COPY GovUk.Education.ExploreEducationStatistics.Content.Security/GovUk.Education.ExploreEducationStatistics.Content.Security.csproj GovUk.Education.ExploreEducationStatistics.Content.Security/


COPY GovUk.Education.ExploreEducationStatistics.Data.Api/GovUk.Education.ExploreEducationStatistics.Data.Api.csproj GovUk.Education.ExploreEducationStatistics.Data.Api/
RUN dotnet restore GovUk.Education.ExploreEducationStatistics.Data.Api/GovUk.Education.ExploreEducationStatistics.Data.Api.csproj

COPY ./GovUk.Education.ExploreEducationStatistics.Common/ ./GovUk.Education.ExploreEducationStatistics.Common/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Model/ ./GovUk.Education.ExploreEducationStatistics.Data.Model/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Services/ ./GovUk.Education.ExploreEducationStatistics.Data.Services/
COPY ./GovUk.Education.ExploreEducationStatistics.Content.Model/ ./GovUk.Education.ExploreEducationStatistics.Content.Model/
COPY ./GovUk.Education.ExploreEducationStatistics.Content.Security/ ./GovUk.Education.ExploreEducationStatistics.Content.Security/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Api/ ./GovUk.Education.ExploreEducationStatistics.Data.Api/

WORKDIR /src/GovUk.Education.ExploreEducationStatistics.Data.Api
RUN dotnet build GovUk.Education.ExploreEducationStatistics.Data.Api.csproj -c Release -o /app/build --no-restore --no-cache

FROM build AS publish
RUN dotnet publish GovUk.Education.ExploreEducationStatistics.Data.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
ARG CERTS_DIR=certs
COPY --from=publish /app/publish .

ENTRYPOINT dotnet GovUk.Education.ExploreEducationStatistics.Data.Api.dll