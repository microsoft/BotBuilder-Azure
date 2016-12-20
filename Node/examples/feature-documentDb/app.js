/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Azure DocumentDb for bot storage. 

# RUN THE BOT:

-Using local DocumentDb emulator:
    -Install DocumentDb emulator (https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator)
    -Start the DocumentDb emulator
    -Set the environment variable NODE_TLS_REJECT_UNAUTHORIZED to the value 0
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed

-Using Azure DocumentDb
    -Create a DocumentDb database (https://azure.microsoft.com/en-us/resources/videos/create-documentdb-on-azure/)
    -Replace host, masterKey, database and collection to your preference in the code below
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed
    
-----------------------------------------------------------------------------*/

var builder = require('botbuilder');
var azure = require('../../');

var documentDbOptions = {
    host: 'https://localhost:8081', // Host for local DocDb emulator
    masterKey: 'C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==', // Fixed key for local DocDb emulator
    database: 'botdocdb',
    collection: 'botdata'
};

var docDbClient = new azure.DocumentDbClient(documentDbOptions);

var tableStorage = new azure.AzureBotStorage({ gzipData: false }, docDbClient);

// Setup bot
var connector = new builder.ConsoleConnector().listen();

// Create your bot with a function to receive messages from the user
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', tableStorage);
