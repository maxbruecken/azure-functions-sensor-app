# Introduction 
This project shows some aspects of programming model of Azure Functions. Common programming techniques like dependency injection or unit testing are parts of this sample project.
The sample function app can collect values from sensors (e.g. air temperature, air pressure, humidity etc.) and store this values. The app observes all known sensors and can detect dead sensors.

# Getting Started
After cloning of this repository you can start the sample function app from your IDE (e.g. Visual Studio or JetBrains Rider). The app can run locally without active Azure subscription, please install Azure Storage Emulator to store all data locally.
You can deploy this sample app to your Azure subscription with provided deploy script (see AzureFunctionApp/AzureFunction.Infrastructure/Templates/deploy.ps1) and start the app in Azure Cloud.
