FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5000

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["GovUk.Education.ExploreEducationStatistics.Data.Api.csproj", "GovUk.Education.ExploreEducationStatistics.Data.Api/"]
RUN dotnet restore "GovUk.Education.ExploreEducationStatistics.Data.Api/GovUk.Education.ExploreEducationStatistics.Data.Api.csproj"
WORKDIR "/src/GovUk.Education.ExploreEducationStatistics.Data.Api"
COPY . .
RUN dotnet build "GovUk.Education.ExploreEducationStatistics.Data.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "GovUk.Education.ExploreEducationStatistics.Data.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GovUk.Education.ExploreEducationStatistics.Data.Api.dll"]