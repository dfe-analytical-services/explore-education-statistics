FROM mcr.microsoft.com/azure-functions/node:2.0

ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
ENV AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://data-storage:10000/devstoreaccount1;QueueEndpoint=http://data-storage:10001/devstoreaccount1; 
ENV AzureWebJobsScriptRoot=/home/site/wwwroot

SHELL ["/bin/bash", "-c"]

WORKDIR /home/site/wwwroot
COPY . .

# func extensions install rather than copying the bin directory
# https://github.com/Azure/azure-functions-docker/issues/35
RUN apt-get update && \
    curl -O https://dot.net/v1/dotnet-install.sh && \
    source dotnet-install.sh --channel Current && \
    rm dotnet-install.sh && \
    npm i -g azure-functions-core-tools@core --unsafe-perm true && \
    func extensions install

RUN npm install

