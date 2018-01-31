/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Azure SQL for bot storage. 

# RUN THE BOT:

-Using Azure SQL
    -Create an Azure SQL database (https://docs.microsoft.com/en-us/azure/sql-database/sql-database-get-started-portal)
    -Replace userName, password, server, database, and table to your preference in the code below.
    -IMPORTANT: You should create your table before attempting to connect your bot to it. 
     If you wish for the table to be created if it isn't initially found, you must add the key-value pair `enforceTable: true` to your SQL configuration. See Line 25.
     The minimal SQL script used to create the table is: `CREATE TABLE your_specified_table_name (id NVARCHAR(200), data NVARCHAR(1000), isCompressed BIT`
    -Set rowCollectionOnRequestCompletion to true to retrieve row content.
    -Run the bot from the command line using "node app.js"
    -Type anything, and the bot will respond showing the text you typed
    
-----------------------------------------------------------------------------*/

var builder = require('botbuilder');
var azure = require('../../');
var restify = require('restify');

var sqlConfig = {
    userName: '<Your-DB-Login>',
    password: '<Your-DB-Login-Password>',
    server: '<Host>',
    // enforceTable: true, // If this property is not set to true it defaults to false. When false if the specified table is not found, the bot will throw an error.
    options: {
        database: '<DB-Name>',
        table: '<DB-Table>',
        encrypt: true,
        rowCollectionOnRequestCompletion: true
    }
}

var sqlClient = new azure.AzureSqlClient(sqlConfig);

var sqlStorage = new azure.AzureBotStorage({ gzipData: false }, sqlClient);

var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', sqlStorage);

var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

server.post('/api/messages', connector.listen());