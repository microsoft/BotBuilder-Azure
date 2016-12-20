/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Azure DocumentDb for bot storage. 

# RUN THE BOT:

-Using local Azure Storage emulator:
    -Install Azure Storage emulator (https://docs.microsoft.com/en-us/azure/storage/storage-use-emulator)
    -Start the Azure Storage emulator
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed

-Using Azure Table
    -Create an Azure storage account (https://docs.microsoft.com/en-us/azure/storage/storage-create-storage-account)
    -Pass your storage account name and key to the AzureTableClient constructor
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed
    
-----------------------------------------------------------------------------*/

var builder = require('botbuilder');
var azure = require('../../');

var tableName = 'BotStore';

var azureTableClient = new azure.AzureTableClient(tableName);

var tableStorage = new azure.AzureBotStorage({ gzipData: false }, azureTableClient);

// Setup bot
var connector = new builder.ConsoleConnector().listen();

// Create your bot with a function to receive messages from the user
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', tableStorage);
