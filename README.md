# V3 Deprecation Notification

Microsoft Bot Framework SDK V4 was released in September 2018, and since then we have shipped a few dot-release improvements. As announced previously, the V3  SDK is being retired with final long-term support ending on December 31st, 2019.
Accordingly, there will be no more development in this repo. **Existing V3 bot workloads will continue to run without interruption. We have no plans to disrupt any running workloads**.

We highly recommend that you start migrating your V3 bots to V4. In order to support this migration we have produced migration documentation and will provide extended support for migration initiatives (via standard channels such as Stack Overflow and Microsoft Customer Support).

For more information please refer to the following references:
* Migration Documentation: https://aka.ms/v3v4-bot-migration
* End of lifetime support announcement: https://aka.ms/bfmigfaq
* Primary V4 Repositories to develop Bot Framework bots
  * [Botbuilder for dotnet](https://github.com/microsoft/botbuilder-dotnet)
  * [Botbuilder for JS](https://github.com/microsoft/botbuilder-js) 
* QnA Maker Libraries were replaced with the following V4 libraries:
  * [Libraries for dotnet](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/Microsoft.Bot.Builder.AI.QnA)
  * [Libraries for JS](https://github.com/Microsoft/botbuilder-js/blob/master/libraries/botbuilder-ai/src/qnaMaker.ts)
* Azure Libraries were replaced with the following V4 libraries:
  * [Botbuilder for JS Azure](https://github.com/Microsoft/botbuilder-js/tree/master/libraries/botbuilder-azure)
  * [Botbuilder for dotnet Azure](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/Microsoft.Bot.Builder.Azure)


# Bot Builder SDK Azure Extensions

The Microsoft Bot Builder SDK Azure Extensions allow for interactions with specific Azure components.

## Bot Builder SDK

For More information about the Bot Builder SDK, which is one of the three main components of the Microsoft Bot Framework, please **[Review the documentation](http://docs.botframework.com)**.

The Microsoft Bot Framework provides just what you need to build and connect intelligent bots that interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.

## Azure Extensions

### Bot Azure Storage

The Bot Builder SDK Azure Extensions enable bot developers to integrate bots with specific Azure components. 

* Azure Table Storage: Allows bot developers to store bot state in their own Azure Storage accounts. For more information on Azure Table Storage, visit the **[Azure Table Storage 
Documentation](https://azure.microsoft.com/en-us/services/storage/tables/)**
* Azure CosmosDB: Allows bot developers to store bot state in CosmosDB. For more information on Azure CosmosDb, visit the **[Azure CosmosDB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction)**

### Azure Table Logging

The Bot Builder SDK Azure Extensions also include [TableLogger](CSharp/Library/Microsoft.Bot.Builder.Azure/TableLogger.cs), an implementation of IActivityLogger which will log activities to the specified Azure Table.

## Azure Extensions Samples

Get started quickly with our samples:

* Azure Table [C#](CSharp/Samples/AzureTable) [Node.js](Node/examples/feature-azureTable)
* CosmosDb [C#](CSharp/Samples/DocumentDb) [Node.js](Node/examples/feature-documentDb)

See all the support options **[here](https://docs.botframework.com/en-us/support/)**.

## Published Libraries
* Nuget package for .NET [Microsoft.Bot.Builder.Azure](https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/) 
* NPM package for nodejs [botbuilder-azure](https://www.npmjs.com/package/botbuilder-azure)

## Botbuilder v4

* V4 for botbuilder-azure [Node.js](https://github.com/Microsoft/botbuilder-js/tree/master/libraries/botbuilder-azure)
* V4 for botbuilder-azure [CSharp](https://github.com/Microsoft/botbuilder-dotnet/tree/master/libraries/Microsoft.Bot.Builder.Azure)
