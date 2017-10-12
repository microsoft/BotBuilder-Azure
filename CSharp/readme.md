# Bot Builder Azure Extensions for .NET
Bot Builder Azure Extensions for .NET enable bot developers to integrate bots with specific Azure components. 



## High level features:

* Azure Table Storage: Allows bot developers to store bot state in their own Azure Storage accounts. For more information on Azure Table Storage, visit the **[Azure Table Storage Documentation](https://azure.microsoft.com/en-us/services/storage/tables/)**
* Azure CosmosDb: Allows bot developers to store bot state in CosmosDb. For more information on Azure CosmosDb, visit the **[Azure CosmosDB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction)**
* Azure SQL Database: Allows bot developers to store bot state using Azure SQL. For more information on Azure SQL, visit the  **[Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/sql-database/)** Note: A script for creating the SqlBotDataEntities table can be found [here](AzureSql-CreateTable.sql)
 
## Creating a Bot Project

Bot project templates for Visual Studio can be downloaded from [here](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-quickstart)

## Sample

You can also base your bot on one of our samples, which showcase using [Azure Table Storage](samples/AzureTable/), [CosmosDb](samples/DocumentDb/) and [Azure SQL](samples/AzureSql/). Read the detailed steps in the sample to set up storage emulators or connect to Azure.

## Test your bot
Use the [Bot Framework Emulator](http://docs.botframework.com/connector/tools/bot-framework-emulator/) to test your bot on localhost. The emulator can be downloaded from [here](http://aka.ms/bf-bc-emulator) 

## Publish your bot
Register your bot with the [Bot Framework](http://docs.botframework.com/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework) then deploy. If you're deploying your bot to Microsoft Azure you can use this great guide: [Deploy a .NET bot to Azure from Visual Studio](https://docs.microsoft.com/en-us/bot-framework/deploy-dotnet-bot-visual-studio).

NOTE: Once you register your bot with the Bot Framework you'll want to update the MicrosoftAppId & MicrosoftAppPassword in the Web.config with the values assigned to you by the portal.

## Bot Framework

The Microsoft Bot Framework provides just what you need to build and connect intelligent bots that interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.


## Dive deeper
Learn how to build great bots.

* [Core Concepts Guide](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-concepts)
* [Bot Builder for .NET Reference](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-overview)
