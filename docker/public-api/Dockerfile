FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY . .
ENTRYPOINT ["dotnet", "GovUk.Education.ExploreEducationStatistics.Public.Data.Api.dll"]
