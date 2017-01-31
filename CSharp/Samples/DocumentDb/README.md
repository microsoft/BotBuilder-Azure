# Azure DocumentDB Sample

This simple echo bots illustrates how to use your own Azure DocumentDB to store the bot state.

To use DocumentDB Store, we configure the Autofac Dependency Injection in [Global.asax](Global.asax.cs). Particularly the following is the piece of code that configures injection of DocumentDB:

```csharp
var store = new DocumentDbBotDataStore(docDbEmulatorUri, docDbEmulatorKey);
builder.Register(c => store)
    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
    .AsSelf()
    .SingleInstance();
```

Note that instead of passing ```docDbEmulatorUri, docDbEmulatorKey```, real Azure DocumentDB credentials can be passed to configure an Azure DocumentDB account.

