/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Azure SQL for bot storage. 

# RUN THE BOT:

-Using Azure SQL
    -Create an Azure SQL database (https://docs.microsoft.com/en-us/azure/sql-database/sql-database-get-started-portal)
    -Replace userName, password, server, database, and table to your preference in the code below.
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