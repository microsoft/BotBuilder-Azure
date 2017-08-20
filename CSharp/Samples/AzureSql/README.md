# Azure Sql Sample

This simple echo bots illustrates how to use your own Azure Sql Storage to store the bot state.

To use Azure Sql, we configure the Autofac Dependency Injection in [Global.asax](Global.asax.cs). Particularly the following is the piece of code that configures injection of Azure Sql Storage:

```csharp
var store = new SqlBotDataStore(ConfigurationManager.ConnectionStrings["BotDataContextConnectionString"].ConnectionString);
builder.Register(c => store)
    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
    .AsSelf()
    .SingleInstance();
```

Note that the connection string should be in the web.config:

```xml
  <connectionStrings>
    <add name="BotDataContextConnectionString" 
        providerName="System.Data.SqlClient" 
        connectionString="Server=tcp:[YourDatabaseServerName].database.windows.net,1433;Initial Catalog=[YourDatabaseName];Persist Security Info=False;User ID=[YourDatabaseUserId];Password=[YourDatabaseUserPassword];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" />
  </connectionStrings>
```