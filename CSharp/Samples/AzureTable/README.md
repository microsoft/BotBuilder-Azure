# Azure Table Sample

This simple echo bots illustrates how to use your own Azure Table Storage to store the bot state.

To use Azure Table Store, we configure the Autofac Dependency Injection in [Global.asax](Global.asax.cs). Particularly the following is the piece of code that configures injection of Azure Table Storage:

```csharp
var store = new TableBotDataStore(CloudStorageAccount.DevelopmentStorageAccount);
builder.Register(c => store)
    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
    .AsSelf()
    .SingleInstance();
```

Note that instead of passing ```csharp CloudStorageAccount.DevelopmentStorageAccount```, real Azure Storage Table credentials can be passed to configure an Azure storage account.

