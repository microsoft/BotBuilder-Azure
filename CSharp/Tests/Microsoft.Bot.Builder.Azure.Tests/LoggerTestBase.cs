using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Azure.Tests;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    public class LoggerTestBase
    {
        public LoggerTestBase()
        {
            DefaultChannel = "channel1";
            DefaultConversation = "conversation1";
            DefaultFrom = "user1";
            DefaultBot = "bot1";
            LastActivity = DateTime.UtcNow;
        }


        private string GetParameterValue(string parameter, string defaultParameter)
        {
            if (parameter == "")
            {
                return defaultParameter;
            }
            else
            {
                return parameter;

            }

        }


        protected IActivity MakeActivity(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "",
            string channel = "",
            string conversation = "",
            string from = "",
            string to = "")
        {
            type = GetParameterValue(type, "message");
            channel = GetParameterValue(channel, DefaultChannel);
            conversation = GetParameterValue(conversation, DefaultConversation);
            from = GetParameterValue(from, DefaultFrom);
            to = GetParameterValue(to, DefaultBot);

            LastActivity += TimeSpan.FromSeconds(increment);
            return new Activity
            {
                Timestamp = LastActivity,
                Type = type,
                ChannelId = channel,
                Conversation = new ConversationAccount(id: conversation),
                From = new ChannelAccount(id: from, name: from),
                Recipient = new ChannelAccount(id: to, name: to),
                Text = text,
                Attachments = attachments
            };
        }

        public string DefaultBot { get; set; }

        public string DefaultFrom { get; set; }

        public string DefaultConversation { get; set; }

        public string DefaultChannel { get; set; }

        public DateTime LastActivity { get; set; }

        protected IActivity ToUser(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "",
            string channel = "",
            string conversation = "")
        {
            channel = GetParameterValue(channel, DefaultChannel);
            conversation = GetParameterValue(conversation, DefaultConversation);
            type = GetParameterValue(type, "message");

            return MakeActivity(text, attachments, increment, type, channel, conversation, DefaultBot, DefaultFrom);
        }

        protected IActivity ToBot(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "",
            string channel = "",
            string conversation = "")
        {
            channel = GetParameterValue(channel, DefaultChannel);
            conversation = GetParameterValue(conversation, DefaultConversation);
            type = GetParameterValue(type, "message");

            return MakeActivity(text, attachments, increment, type, channel, conversation, DefaultFrom, DefaultBot);
        }

        protected IEnumerable<IActivity> Filter(IEnumerable<IActivity> activities, string channel = "", string conversation = "",
            string fromId = null, string toId = null,
            int? max = null, DateTime oldest = default(DateTime),
            int? take = null)
        {

            channel = GetParameterValue(channel, DefaultChannel);
            conversation = GetParameterValue(conversation, DefaultConversation);

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


        protected void AssertEqual(IEnumerable<IActivity> expected, IEnumerable<IActivity> actual)
        {
            var exp = expected.ToList();
            var act = actual.ToList();
            Assert.IsTrue(exp.SequenceEqual(act, new CompareActivity()));
        }

        protected List<IActivity> GetTestActivityList()
        {
            // Message bigger than one block
            var chars = new char[100000];
            var rand = new Random();
            for (var i = 0; i < chars.Length; ++i)
            {
                chars[i] = (char)('0' + rand.Next(74));
            }

            var activities = new List<IActivity>
            {
                // Bigger than one property
                ToUser(new string(chars)),
                ToBot("Hi"),
                ToUser("Welcome to the bot"),
                ToBot("Weather"),
                ToUser("or not"),
                // Make sure auto-increment works
                ToUser("right away", increment: 0),
                ToUser("another conversation", conversation: "conversation2"),
                ToUser("somewhere else", channel: "channel2"),
                MakeActivity("to someone else", to: "user2"),
                MakeActivity("from someone else", @from: "user2"),
                MakeActivity("to someone else in another conversation", to: "user2", conversation: "conversation3"),
                MakeActivity("from someone else on another channel", @from: "user2", channel: "channel3"),
                ToBot("sometime later", increment: 180)
            };
            return activities;
        }
    }
}