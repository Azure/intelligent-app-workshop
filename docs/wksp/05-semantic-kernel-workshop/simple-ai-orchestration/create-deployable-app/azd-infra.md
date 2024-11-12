# Azure Developer CLI (AZD) and Bicep templates for Infrastructure as Code deployment

In this section we go over the creation of `azd` [(Azure Developer CLI)]((https://aka.ms/azure-dev/install))
files and corresponding bicep templates to be able to build, provision infrastructure and
deploy our solution into Azure.

## Azure Developer CLI files overview

The following components are needed to deploy via AZD

### `azure.yaml` file

This file specifies the components to be deployed. Our file specifies two components to be deployed:

1. Application backend - .NET deployed as Azure Container App
1. Web frontend - Typescript deployed as Azure Container App

For each component we need to provide the path to the corresponding `Dockerfile` which will be used
to build and package the application:

```yaml
name: semantic-kernel-workshop-csharp
metadata:
  template: semantic-kernel-workshop-csharp@0.0.1-beta
services:
  api:
    project: ./App/backend/
    host: containerapp
    language: dotnet
    docker:
      path: ../Dockerfile
      context: ../../
  web:
    project: ../frontend/
    host: containerapp
    language: ts
    docker:
      path: ./Dockerfile
      context: ./
```

### `infra` directory

Within the `infra` directory you have the option to provide either `bicep` or `terraform` templates
to deploy the required infrastructure for our application to run. In this example we
use `bicep` templates which are organized as follows:

* **infra**
  * `main.bicep` - contains the bicep parameters and modules to deploy
  * `main.paramters.json` - parameter values to be used during deployment
  * `abbreviations.json` - optional file to specify suffix abbreviations for each resource type
    * app - subdirectory with application related templates
      * `api.bicep` - bicep template for backend application infrastructure
      * `web.bicep` - bicep templated for web application infrastructure
    * core - subdirectory with templates for core infrastructure components
      * **ai** - subdirectory for AI related components
      * **host** - subdirectory for container app, environment and registry components
      * **monitor** - subdirectory for monitoring components (e.g. application insights)
      * **security** - subdirectory for security components (e.g. keyvault)
      * **storage** - subdirectory for storage components (e.g. storage account)

The `azd init` command can be used to generate a starter template, however the quickest way
to generate an existing template is to find a template that uses similar components from
[awesome-azd](https://azure.github.io/awesome-azd/).

## Deploying using AZD CLI

You can build, provision all resources and deploy by following these steps:

1. Switch to `workshop/donet` directory.
1. Ensure Docker desktop is running.
1. Run `azd auth login` to login to your Azure account.
1. Run `azd up` to provision Azure resources and deploy this sample to those resources.
   You will be prompted for the following parameters:
    * Environment name: sk-test
    * Select an Azure subscription to use from list
    * Select an Azure location to use: e.g. (US) East US 2 (eastus2)
    * Enter a value for the infrastructure parameters:
      * 'openAIApiKey'
      * 'openAiChatGptDeployment': e.g. gpt-4o
      * 'openAiEndpoint'
      * 'stockServiceApiKey'
1. After the application has been successfully deployed you will see the API and Web Service URLs printed in the console.  
   Click the Web Service URL to interact with the application in your browser.

   **NOTE:** It may take a few minutes for the application to be fully  deployed.

## Deployment removal

In order to remove all resources deployed, use this command:

```bash
azd down --purge
```
