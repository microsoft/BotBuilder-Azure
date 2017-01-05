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
using Microsoft.Bot.Builder.Azure.Tests;
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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class LoggerTests
    {
        private const string _defaultChannel = "channel1";
        private const string _defaultConversation = "conversation1";
        private const string _defaultUser = "user1";
        private const string _defaultBot = "bot1";
        private DateTime _lastActivity = DateTime.UtcNow;

        private IActivity MakeActivity(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation,
            string from = _defaultUser,
            string to = _defaultBot)
        {
            _lastActivity += TimeSpan.FromSeconds(increment);
            return new Activity
            {
                Timestamp = _lastActivity,
                Type = type,
                ChannelId = channel,
                Conversation = new ConversationAccount(id: conversation),
                From = new ChannelAccount(id: from, name: from),
                Recipient = new ChannelAccount(id: to, name: to),
                Text = text,
                Attachments = attachments
            };
        }

        private IActivity ToUser(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation)
        {
            return MakeActivity(text, attachments, increment, type, channel, conversation, _defaultBot, _defaultUser);
        }

        private IActivity ToBot(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation)
        {
            return MakeActivity(text, attachments, increment, type, channel, conversation, _defaultUser, _defaultBot);
        }

        private IEnumerable<IActivity> Filter(IEnumerable<IActivity> activities, string channel = _defaultChannel, string conversation = _defaultConversation,
            string fromId = null, string toId = null,
            int? max = null, DateTime oldest = default(DateTime),
            int? take = null)
        {
            return (from activity in activities.Take(take ?? activities.Count())
                    where activity.Timestamp >= oldest
                    && (channel == null || activity.ChannelId == channel)
                    && (conversation == null || activity.Conversation.Id == conversation)
                    && (fromId == null || activity.From.Id == fromId)
                    && (toId == null || activity.Recipient.Id == toId)
                    select activity)
                    .Take(max ?? int.MaxValue);
        }

        public class CompareActivity : IEqualityComparer<IActivity>
        {
            public bool Equals(IActivity x, IActivity y)
            {
                var m1 = (IMessageActivity)x;
                var m2 = (IMessageActivity)y;
                return m1.ChannelId == m2.ChannelId
                    && m1.Conversation.Id == m2.Conversation.Id
                    && m1.From.Id == m2.From.Id
                    && m1.Recipient.Id == m2.Recipient.Id
                    && m1.Timestamp == m2.Timestamp
                    && m1.Text == m2.Text
                    // && m1.Attachments == m2.Attachments
                    && m1.Type == m2.Type;
            }

            public int GetHashCode(IActivity obj)
            {
                throw new NotImplementedException();
            }
        }

        [AssemblyInitialize]
        public static void ClassInitialize(TestContext context)
        {
            StorageEmulatorRunner.Start();
        }

        [AssemblyCleanup]
        public static void ClassCleanup()
        {
            StorageEmulatorRunner.Stop();
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

            // Message bigger than one block
            var chars = new char[100000];
            var rand = new Random();
            for (var i = 0; i < chars.Length; ++i)
            {
                chars[i] = (char)('0' + rand.Next(74));
            }

            var activities = new List<IActivity>
            {
                ToBot("Hi"),
                ToUser("Welcome to the bot"),
                ToBot("Weather"),
                ToUser("or not"),
                // Make sure auto-increment works
                ToUser("right away", increment:0),
                // Bigger than one property
                ToUser(new string(chars)),
                ToUser("another conversation", conversation:"conversation2"),
                ToUser("somewhere else", channel:"channel2"),
                MakeActivity("to someone else", to:"user2"),
                MakeActivity("from someone else", from:"user2"),
                MakeActivity("to someone else in another conversation", to:"user2", conversation:"conversation3"),
                MakeActivity("from someone else on another channel", from:"user2", channel:"channel3"),
                ToBot("sometime later", increment:180)
            };
            var comparator = new CompareActivity();
            for (var i = 0; i < activities.Count; ++i)
            {
                await logger.LogAsync(activities[i]);
                var oldest = _lastActivity.AddSeconds(-30);
                AssertEqual(Filter(activities, oldest: oldest, take: i + 1), source.Activities(_defaultChannel, _defaultConversation, oldest));
            }

            var conversation = Filter(activities);
            AssertEqual(conversation, source.Activities(_defaultChannel, _defaultConversation));
            AssertEqual(Filter(activities, channel: "channel2"), source.Activities("channel2", "conversation1"));
            AssertEqual(Filter(activities, conversation: "conversation2"), source.Activities(_defaultChannel, "conversation2"));

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
            await source.WalkActivitiesAsync(replay.Replay, _defaultChannel, _defaultConversation);

            await manager.DeleteConversationAsync(_defaultChannel, "conversation2");
            Assert.AreEqual(0, source.Activities(_defaultChannel, "conversation2").Count());

            await manager.DeleteConversationAsync("channel2", _defaultConversation);
            Assert.AreEqual(0, source.Activities("channel2", _defaultConversation).Count());

            await manager.DeleteUserActivitiesAsync("user2");
            await source.WalkActivitiesAsync(activity =>
            {
                Assert.IsTrue(activity.From.Id != "user2" && activity.Recipient.Id != "user2");
                return Task.CompletedTask;
            });

            var purge = _lastActivity.AddSeconds(-30.0);
            await manager.DeleteBeforeAsync(purge);
            AssertEqual(Filter(activities, oldest: purge), source.Activities(_defaultChannel, _defaultConversation));
        }

        private void AssertEqual(IEnumerable<IActivity> expected, IEnumerable<IActivity> actual)
        {
            var exp = expected.ToList();
            var act = actual.ToList();
            Assert.IsTrue(exp.SequenceEqual(act, new CompareActivity()));
        }
    }
}
