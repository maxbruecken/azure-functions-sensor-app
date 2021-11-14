FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet6-appservice
COPY / /home/site/wwwroot
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
