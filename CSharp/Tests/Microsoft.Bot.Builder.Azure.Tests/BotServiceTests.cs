using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class BotServiceTests : DialogTestBase
    {
        [TestMethod]
        public async Task UseTableStorage_Test()
        {
            var oldValue = Environment.GetEnvironmentVariable(AppSettingKeys.UseTableStorageForConversationState);
            // set the environment variable so bot uses table storage as state store
            System.Environment.SetEnvironmentVariable(AppSettingKeys.UseTableStorageForConversationState, true.ToString());
            bool shouldUse = false;
            // assert that UseTableStorage is set to true
            Assert.IsTrue(bool.TryParse(Utils.GetAppSetting(AppSettingKeys.UseTableStorageForConversationState), out shouldUse) && shouldUse);

            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));
                builder
                    .RegisterInstance(echo)
                    .As<IDialog<object>>();

                // register the development storage as the storage for TableBotDataStore
                builder.Register(c => new TableBotDataStore(CloudStorageAccount.DevelopmentStorageAccount))
                    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                    .AsSelf()
                    .SingleInstance();

                builder.Update(container);


                var toBot = MakeTestMessage();

                toBot.Text = "hello";

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }


                var queue = container.Resolve<Queue<IMessageActivity>>();
                Assert.AreEqual("echo: hello", queue.Dequeue().Text);

                IBotDataStore<BotData> tableStore = container.Resolve<TableBotDataStore>();
                var privateConversationData = await tableStore.LoadAsync(Address.FromActivity(toBot),
                    BotStoreType.BotPrivateConversationData, CancellationToken.None);
                Assert.IsNotNull(privateConversationData.Data);
            }

            Environment.SetEnvironmentVariable(AppSettingKeys.UseTableStorageForConversationState, oldValue);
        }
    }
}
