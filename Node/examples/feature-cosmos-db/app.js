/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Azure Cosmos DB for bot storage. 

# RUN THE BOT:

-Using local Cosmos DB emulator:
    -Install Cosmos DB emulator (https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
    -Start the Cosmos DB emulator
    -Set the environment variable NODE_TLS_REJECT_UNAUTHORIZED to the value 0
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed

-Using Azure Cosmos DB
    -Create a Cosmos DB database (https://docs.microsoft.com/en-us/azure/cosmos-db/documentdb-nodejs-get-started)
    -Replace host, masterKey, database and collection to your preference in the code below
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed
    
-----------------------------------------------------------------------------*/

var builder = require('botbuilder');
var azure = require('../../');
var restify = require('restify');

var cosmosDbOptions = {
    host: 'https://localhost:8081', // Host for local Cosmos DB emulator
    masterKey: 'C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==', // Fixed key for local Cosmos DB emulator
    database: 'botcosmosdb',
    collection: 'botdata'
};

var cosmosDbClient = new azure.CosmosDbClient(cosmosDbOptions);

var tableStorage = new azure.AzureBotStorage({ gzipData: false }, cosmosDbClient);

var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', tableStorage);

var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

server.post('/api/messages', connector.listen());

