# Bot Builder Azure Extensions for Node.js
Bot Builder Azure Extensions for Node.js enable bot developers to integrate bots with specific Azure components. 



## High level features:

* Azure Table Storage: Allows bot developers to store bot state in their own Azure Storage accounts. For more information on Azure Table Storage, visit the **[Azure Table Storage Documentation](https://azure.microsoft.com/en-us/services/storage/tables/)**
* Azure DocumentDB: Allows bot developers to store bot state in DocumentDB. For more information on Azure DocumentDB, visit the **[Azure DocumentDB Documentation](https://azure.microsoft.com/en-us/services/documentdb/)**
 
## Prepare to Build a Bot

Create a folder for your bot, cd into it, and run npm init.

    npm init
    
Get the BotBuilder, BotBuilder-Azure and Restify modules using npm.

    npm install --save botbuilder
    npm install --save botbuilder-azure
    npm install --save restify

## Sample

You can base your first bot on one of our samples, which showcase using [Azure Table Storage](examples/feature-azureTable/app.js) and [DocumentDb](examples/feature-documentDb/app.js). Read the detailed steps in the sample to set up storage emulators or connect to Azure.

## Test your bot
Use the [Bot Framework Emulator](http://docs.botframework.com/connector/tools/bot-framework-emulator/) to test your bot on localhost. 

Install the emulator from [here](http://aka.ms/bf-bc-emulator) and then start your bot in a console window.

    node app.js
    
Start the emulator and say "hello" to your bot.

## Publish your bot
Deploy your bot to the cloud and then [register it](http://docs.botframework.com/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework) with the Microsoft Bot Framework. If you're deploying your bot to Microsoft Azure you can use this great guide for [Publishing a Node.js app to Azure using Continuous Integration](https://blogs.msdn.microsoft.com/sarahsays/2015/08/31/building-your-first-node-js-app-and-publishing-to-azure/).

NOTE: When you register your bot with the Bot Framework you'll want to update the appId & appSecret for both your bot and the emulator with the values assigned to you by the portal.

## Bot Framework

The Microsoft Bot Framework provides just what you need to build and connect intelligent bots that interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

![Bot Framework Diagram](http://docs.botframework.com/en-us/images/faq-overview/botframework_overview_july.png)

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.


## Dive deeper
Learn how to build great bots.

* [Core Concepts Guide](http://docs.botframework.com/builder/node/guides/core-concepts/)
* [Bot Builder for Node.js Reference](http://docs.botframework.com/sdkreference/nodejs/modules/_botbuilder_d_.html)
