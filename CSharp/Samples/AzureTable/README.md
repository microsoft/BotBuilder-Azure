# Alarm Bot Sample

The alarm bot sample illustrates several concepts:

1. modifying the dialog stack based on external events (e.g. the passage of time)
2. proactively messaging the user based on external events (e.g. the alarm is ringing)
3. pervasive IScorable message handling middleware (e.g. live buttons in conversation history)
4. integration of ASP.NET dependency injection with Autofac

The top "composition root" of the alarm code is the Autofac [Alarm Module](Models/AlarmModule.cs).  In this module, you can see how we specify the dialog  and supporting services for our bot.  There are two dialogs:

* [AlarmLuisDialog](Dialogs/AlarmLuisDialog.cs) is the root dialog for the conversation, and it leverages the LUIS built-in model for alarms
* [AlarmRingDialog](Dialogs/AlarmRingDialog.cs) will ask the user if they want to snooze the alarm when it rings

These two dialogs are supported by a series of other classes.

* [Alarm](Models/Alarm.cs) models our primary domain object
* [Alarm Scheduler](Models/AlarmScheduler.cs) will ring an alarm at the proper time by polling the system clock
* [Alarm Service](Models/AlarmService.cs) provides the standard create/read/update/delete operations for alarms
* [Alarm Renderer](Models/AlarmRenderer.cs) will render the alarm's state and buttons for the user
* [Alarm Scorable](Models/AlarmScorable.cs) collaborates with the Alarm Renderer and Alarm Service to allow the user to modify the state of the alarm
* [External Event](Models/ExternalEvent.cs) handles the ringing of an alarm to interrrupt the user's conversation with the AlarmRingDialog

Some of these services should not be serialized with the dialog state, and we mark those services with a key so that they are restored from the Autofac dependency injection container on deserialization.  Other services are dependent on information in the incoming message to the bot, and we mark those services with a lifetime scope corresponding to the incoming message.


