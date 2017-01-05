# Bot Builder SDK Azure Extensions

The Microsoft Bot Builder SDK Azure Extensions allow for interactions with specific Azure components.

## Bot Builder SDK

For More information about the Bot Builder SDK, which is one of the three main components of the Microsoft Bot Framework, please **[Review the documentation](http://docs.botframework.com)**.

The Microsoft Bot Framework provides just what you need to build and connect intelligent bots that interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

![Bot Framework Diagram](http://docs.botframework.com/en-us/images/faq-overview/botframework_overview_july.png)

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.

## Azure Extensions

### Bot Azure Storage

The Bot Builder SDK Azure Extensions enable bot developers to integrate bots with specific Azure components. 

* Azure Table Storage: Allows bot developers to store bot state in their own Azure Storage accounts. For more information on Azure Table Storage, visit the **[Azure Table Storage Documentation](https://azure.microsoft.com/en-us/services/storage/tables/)**
* Azure DocumentDB: Allows bot developers to store bot state in DocumentDB. For more information on Azure DocumentDB, visit the **[Azure DocumentDB Documentation](https://azure.microsoft.com/en-us/services/documentdb/)**

### Azure Table Logging

The Bot Builder SDK Azure Extensions also include [TableLogger](CSharp/Library/Microsoft.Bot.Builder.Azure/TableLogger.cs), an implementation of IActivityLogger which will log activities to the specified Azure Table.

## Azure Extensions Samples

Get started quickly with our samples:

* Azure Table [C#](CSharp/Samples/AzureTable) [Node.js](Node/examples/feature-azureTable)
* DocumentDB [C#](CSharp/Samples/DocumentDb) [Node.js](Node/examples/feature-documentDb)

See all the support options **[here](https://docs.botframework.com/en-us/support/)**.

