FROM node:20 AS nodeStage
WORKDIR /src/ClientApp
COPY explore-education-statistics-admin/. .
RUN pnpm install

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY GovUk.Education.ExploreEducationStatistics.Common/GovUk.Education.ExploreEducationStatistics.Common.csproj GovUk.Education.ExploreEducationStatistics.Common/
COPY GovUk.Education.ExploreEducationStatistics.Content.Model/GovUk.Education.ExploreEducationStatistics.Content.Model.csproj GovUk.Education.ExploreEducationStatistics.Content.Model/
COPY GovUk.Education.ExploreEducationStatistics.Content.Services/GovUk.Education.ExploreEducationStatistics.Content.Services.csproj GovUk.Education.ExploreEducationStatistics.Content.Services/
COPY GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.csproj GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/
COPY GovUk.Education.ExploreEducationStatistics.Data.Services/GovUk.Education.ExploreEducationStatistics.Data.Services.csproj GovUk.Education.ExploreEducationStatistics.Data.Services/
COPY GovUk.Education.ExploreEducationStatistics.Publisher.Model/GovUk.Education.ExploreEducationStatistics.Publisher.Model.csproj GovUk.Education.ExploreEducationStatistics.Publisher.Model/
COPY GovUk.Education.ExploreEducationStatistics.Content.Security/GovUk.Education.ExploreEducationStatistics.Content.Security.csproj GovUk.Education.ExploreEducationStatistics.Content.Security/
COPY GovUk.Education.ExploreEducationStatistics.Data.Model/GovUk.Education.ExploreEducationStatistics.Data.Model.csproj GovUk.Education.ExploreEducationStatistics.Data.Model/
COPY GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.csproj GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/

COPY GovUk.Education.ExploreEducationStatistics.Admin/GovUk.Education.ExploreEducationStatistics.Admin.csproj GovUk.Education.ExploreEducationStatistics.Admin/
RUN dotnet restore GovUk.Education.ExploreEducationStatistics.Admin/GovUk.Education.ExploreEducationStatistics.Admin.csproj

COPY ./GovUk.Education.ExploreEducationStatistics.Admin/ ./GovUk.Education.ExploreEducationStatistics.Admin/

COPY ./GovUk.Education.ExploreEducationStatistics.Common/ ./GovUk.Education.ExploreEducationStatistics.Common/
COPY ./GovUk.Education.ExploreEducationStatistics.Content.Model/ ./GovUk.Education.ExploreEducationStatistics.Content.Model/
COPY ./GovUk.Education.ExploreEducationStatistics.Content.Services/ ./GovUk.Education.ExploreEducationStatistics.Content.Services/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Processor/ ./GovUk.Education.ExploreEducationStatistics.Data.Processor/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Services/ ./GovUk.Education.ExploreEducationStatistics.Data.Services/
COPY ./GovUk.Education.ExploreEducationStatistics.Publisher.Model/ ./GovUk.Education.ExploreEducationStatistics.Publisher.Model/
COPY ./GovUk.Education.ExploreEducationStatistics.Content.Security/ ./GovUk.Education.ExploreEducationStatistics.Content.Security/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Model/ ./GovUk.Education.ExploreEducationStatistics.Data.Model/
COPY ./GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/ ./GovUk.Education.ExploreEducationStatistics.Data.Processor.Model/

WORKDIR /src/GovUk.Education.ExploreEducationStatistics.Admin
RUN dotnet build GovUk.Education.ExploreEducationStatistics.Admin.csproj -c Release -o /app/build --no-restore --no-cache

COPY --from=nodeStage /src/ClientApp wwwroot/

FROM build AS publish
RUN dotnet publish GovUk.Education.ExploreEducationStatistics.Admin.csproj -c Release -o /app/publish --no-restore

FROM base AS final
ARG CERTS_DIR=certs
COPY --from=publish /app/publish .

ENTRYPOINT dotnet GovUk.Education.ExploreEducationStatistics.Admin.dll