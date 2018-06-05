# Azure Analyze My Tweets

[Live Demo](https://aamt-func.azurewebsites.net/)

## Architecture
![alt text](https://github.com/davemurphysf/aamt-spa/raw/master/aamt-diagram.png "aamt diagram")

## Description
This demo consists of two parts: a single-page application (SPA) and a serverless back-end.  This entire demo can be run on a purely consumption basis (i.e. you only pay for what you use).

The Azure services used are:
* [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)
* [Azure Storage (Blobs, Queues & Tables)](https://azure.microsoft.com/en-us/services/storage/)
* [Azure Cognitive Services (Text & Image)](https://azure.microsoft.com/en-us/services/cognitive-services/)
* [Azure SignalR Service](https://azure.microsoft.com/en-us/services/signalr-service/)

The [SPA](https://github.com/davemurphysf/aamt-spa) uses [React](https://reactjs.org) & [Redux](https://redux.js.org/), and was created with [Create React App](https://github.com/facebook/create-react-app).  The [Functions](https://github.com/davemurphysf/aamt-functions) use a mixture of JavaScript and C#.

Optionally, you can use [Visual Studio Team Services](https://www.visualstudio.com/team-services/) for CI/CD from a GitHub or Bitbucket repository for free.
#### Demo Flow
1. Initial request returns SPA from Blob Storage via Functions proxy
2. During the first SPA setup, a request is made to the ```Signin``` function that returns the SignalR connection details and the current stats
3. The user enters a Twitter handle to fetch their recent tweets
   * The return of the ```Fetch Tweets``` function triggers a message to be put on the ```Fetch Queue```.
4. The user clicks buttons to analyze any tweet text or embedded images
   * The return of the ```Analyze Text``` or ```Analyze Image``` function triggers a message to be put on the ```Analyze Queue```.
5. Triggered by the Queue messages, the ```Update Fetch Stats``` function adds an entry to the ```Fetch Table``` and sends the updated stats to ```SignalR``` which broadcasts them to all clients.  Similarly, the ```Update Analyze Stats``` function adds an entry to the ```Analyze Table``` and sends the updated stats to ```SignalR``` which broadcasts them to all clients.

## Setup

1. Create Resource Group
```
az group create \
    --location "$REGION" \
    --name "$RESOURCE_GROUP_FUNC"
```

2. Create Storage accounts

   The function app requires a dedicated storage account to store the function code for the backing AppService.
```
az storage account create \
    -g "$RESOURCE_GROUP_FUNC" \
    -l "$REGION" \
    -n "$STORAGE_NAME" \
    --sku "Standard_LRS"

az storage account create \
    -g "$RESOURCE_GROUP_FUNC" \
    -l "$REGION" \
    -n "$FUNCTION_STORAGE_NAME" \
    --sku "Standard_LRS"
```

3. Create Functions app
```
az functionapp create \
    -c "$REGION" \
    -g "$RESOURCE_GROUP_FUNC" \
    -n "$FUNCTION_NAME" \
    -s "$FUNCTION_STORAGE_NAME"
```

4. Create SignalR Service

   If you haven't already, you'll need to install the SignalR extension to the CLI until the SignalR service goes GA
```
az extension add -n signalr
```
```
az signalr create \
    -g "$RESOURCE_GROUP" \
    -l "$REGION" \
    -n "$SIGNALR_NAME" \
    --sku "Free_DS2"
```

5. Create Cognitive Services endpoints

   Text
```
az cognitiveservices account create \
    -g "$RESOURCE_GROUP" \
    -l "$REGION" \
    -n "$COGNITIVE_SERVICES_TEXT_NAME" \
    --kind "TextAnalytics" \
    --sku "F0" \
    --yes
```

   Computer Vision
```
az cognitiveservices account create \
    -g "$RESOURCE_GROUP" \
    -l "$REGION" \
    -n "$COGNITIVE_SERVICES_IMAGE_NAME" \
    --kind "ComputerVision" \
    --sku "F0" \
    --yes
```

6. Setup Application Settings

   The functions are setup to expect Application Settings/Environment Variables for the following things:

   * BlobHost - Host name for the Blob storage holding the SPA static assets
   * aamt_STORAGE - Connection string to Storage account for Queue triggers   
   * TableConnectionString - Connection string for reading/writing Table entries
   * COMPUTER_VISION_KEY - API Key for Computer Vision Cognitive Services
   * TEXT_ANALYTICS_KEY - API Key for Text Analytics Cognitive Services
   * AzureSignalRConnectionString - Connection String for SignalR Service
   * TWITTER_CONSUMER_KEY - OAuth credentials from Twitter
   * TWITTER_CONSUMER_SECRET - OAuth credentials from Twitter

    You can get most/all of this information from the CLI, but its frankly faster to just use the portal.

7. Add External Javascript packages

   Follow the [directions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference-node#node-version-and-package-management) to upload the ```package.json``` file to the ```wwwroot``` directory in the functions app service and run ```npm install``` so that the JavaScript functions have access to the ```async``` and ```request``` packages.

8. Deploy Function App

    You can use [continuous deployment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment) from a repository, however I just used [zip deployment](https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push) and the CLI for iterating.
```
az functionapp deployment source config-zip\
    -g myResourceGroup \
    -n "$FUNCTION_NAME" \
    --src <zip_file_path>
```

### Considerations
Because this demo is using the Consumption function plan, the inital call is very slow (i.e. cold start) because Azure automatically loads and unloads the backing AppService.  In production systems that have intermittent load, using a dedicated AppService would avoid the cold start latency.
