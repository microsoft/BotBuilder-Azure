// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public sealed class TableLoggerTests : LoggerTestBase
    {
        public TableLoggerTests() : base()
        {
            
        }

        [TestMethod]
        [TestCategory("Azure")]
        // NOTE: To run this test you must have installed the Azure Storage Emulator. 
        // You can download it here: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
        // The test will automatically start the emulator.
        public async Task TableLoggerTest()
        {
            var tableName = "TableLoggerTestActivities";
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            account.CreateCloudTableClient().GetTableReference(tableName).DeleteIfExists();
            var builder = new ContainerBuilder();
            builder.RegisterModule(new TableLoggerModule(account, tableName));
            var container = builder.Build();
            var logger = container.Resolve<IActivityLogger>();
            var source = container.Resolve<IActivitySource>();
            var manager = container.Resolve<IActivityManager>();

            var activities = GetTestActivityList();

            var comparator = new CompareActivity();
            for (var i = 0; i < activities.Count; ++i)
            {
                await logger.LogAsync(activities[i]);
                var oldest = LastActivity.AddSeconds(-30);
                AssertEqual(Filter(activities, oldest: oldest, take: i + 1), source.Activities(DefaultChannel, DefaultConversation, oldest));
            }

            var conversation = Filter(activities);
            AssertEqual(conversation, source.Activities(DefaultChannel, DefaultConversation));
            AssertEqual(Filter(activities, channel: "channel2"), source.Activities("channel2", "conversation1"));
            AssertEqual(Filter(activities, conversation: "conversation2"), source.Activities(DefaultChannel, "conversation2"));

            var transcript = new List<string>();
            foreach (var activity in conversation)
            {
                var msg = activity as IMessageActivity;
                if (msg != null)
                {
                    transcript.Add($"({msg.From.Name} {msg.Timestamp:g})");
                    transcript.Add($"{msg.Text}");
                }
            }
            int j = 0;
            var botToUser = new Mock<IBotToUser>();
            botToUser
                .Setup(p => p.PostAsync(It.IsAny<IMessageActivity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback((IMessageActivity activity, CancellationToken cancel) =>
                    Assert.AreEqual(transcript[j++], activity.Text));
            botToUser
                .Setup(p => p.MakeMessage())
                .Returns(new Activity());
            var replay = new ReplayTranscript(botToUser.Object);
            await source.WalkActivitiesAsync(replay.Replay, DefaultChannel, DefaultConversation);

            await manager.DeleteConversationAsync(DefaultChannel, "conversation2");
            Assert.AreEqual(0, source.Activities(DefaultChannel, "conversation2").Count());

            await manager.DeleteConversationAsync("channel2", DefaultConversation);
            Assert.AreEqual(0, source.Activities("channel2", DefaultConversation).Count());

            await manager.DeleteUserActivitiesAsync("user2");
            await source.WalkActivitiesAsync(activity =>
            {
                Assert.IsTrue(activity.From.Id != "user2" && activity.Recipient.Id != "user2");
                return Task.CompletedTask;
            });

            var purge = LastActivity.AddSeconds(-30.0);
            await manager.DeleteBeforeAsync(purge);
            AssertEqual(Filter(activities, oldest: purge), source.Activities(DefaultChannel, DefaultConversation));
        }


    }
}
