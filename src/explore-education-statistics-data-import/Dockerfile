FROM mcr.microsoft.com/azure-functions/node:2.0

ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
ENV AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://data-storage:10000/devstoreaccount1;QueueEndpoint=http://data-storage:10001/devstoreaccount1; 
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
COPY . /home/site/wwwroot
