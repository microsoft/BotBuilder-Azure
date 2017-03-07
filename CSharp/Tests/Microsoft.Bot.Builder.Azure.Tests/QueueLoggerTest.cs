using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.History;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Bot.Builder.Tests
{

    enum QueueKind
    {
        ServiceBus,
        AzureQueue
    }

    [TestClass]
    public sealed class QueueLoggerTest : LoggerTestBase
    {
        async Task RunTestCase(bool compressed, LargeMessageMode handlingMode, QueueKind kind)
        {
            IContainer container;
            IQueueReader reader;
            int batchCount;
            var queueSettings = new QueueLoggerSettings
            {
                CompressMessage = compressed,
                LargeMessageHandlingPattern = handlingMode
            };

            IActivityLogger logger;
            if (kind == QueueKind.ServiceBus)
            { 
                container = RegisterServiceBusContainer(out logger, queueSettings);
                reader = new ServiceBusQueueReader(container.Resolve<QueueClient>(), queueSettings);
                batchCount = 1000;
            }
            else
            { 
                container = RegisterAzureQueueContainer(out logger, queueSettings);
                reader = new AzureQueueReader(container.Resolve<CloudQueue>(), queueSettings);
                batchCount = 30;
            }
            await LogItems(logger, container, queueSettings, reader, batchCount);
        }


        [TestMethod]
        [TestCategory("Azure")]
        public async Task ServiceBusTestUnCompressed()
        {
            bool exceptionHappened = false;

            await RunTestCase(false, LargeMessageMode.Trim, QueueKind.ServiceBus);
            await RunTestCase(false, LargeMessageMode.Discard, QueueKind.ServiceBus);

            //expect an exception
            try
            {
                await RunTestCase(false, LargeMessageMode.Error, QueueKind.ServiceBus);
            }
            catch
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened,"Failed to throw exception on large uncompressed message");
        }

        [TestMethod]
        [TestCategory("Azure")]
        public async Task ServiceBusTestCompressed()
        {
            bool exceptionHappened = false;

            await RunTestCase(true, LargeMessageMode.Trim, QueueKind.ServiceBus);
            await RunTestCase(true, LargeMessageMode.Discard, QueueKind.ServiceBus);

            //expect an exception
            try
            {
                await RunTestCase(true, LargeMessageMode.Error, QueueKind.ServiceBus);
            }
            catch 
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened, "Failed to throw exception on large compressed message");
        }

        private static IContainer RegisterServiceBusContainer(out IActivityLogger logger, QueueLoggerSettings queueSettings)
        {
            var queueName = "mytestqueue";
            var connectionstring =
                "Endpoint=sb://moduletestsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=m0nvZq6baRMmPm2T4JFIAOs8G2vpJrnz9QKUp1HIrVs=";

            //delete as part of the test
            NamespaceManager namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionstring);

            namespaceManager.DeleteQueue(queueName);

            var builder = new ContainerBuilder();

            builder.RegisterModule(new QueueActivityModule(connectionstring, queueName, queueSettings));
            var container = builder.Build();
            logger = container.Resolve<IActivityLogger>();

            return container;
        }

        private async Task LogItems(IActivityLogger logger, IContainer container, QueueLoggerSettings loggerSettings, IQueueReader reader, int batchCount)
        {
            //write all messages
            var activities = GetTestActivityList();

            for (var i = 0; i < activities.Count; ++i)
            {
                await logger.LogAsync(activities[i]);
            }

            //now read all messages, using reader
            var readActivities = reader.ReadBatch(batchCount);

            //make sure we got the same number we sent in.
            if (loggerSettings.LargeMessageHandlingPattern != LargeMessageMode.Discard)
                Assert.AreEqual(activities.Count, readActivities.Count);
            else
                //here we expect that one message will be dropped.
                Assert.AreEqual(activities.Count, readActivities.Count+1);
        }

        private static IContainer RegisterAzureQueueContainer(out IActivityLogger logger, QueueLoggerSettings queueSettings)
        {
            var queueName = "myquename";
            var account = CloudStorageAccount.DevelopmentStorageAccount;

            //delete as part of the test
            account.CreateCloudQueueClient().GetQueueReference(queueName).DeleteIfExists();
            
            var builder = new ContainerBuilder();

            builder.RegisterModule(new QueueActivityModule(account, queueName,queueSettings));
            var container = builder.Build();

            logger = container.Resolve<IActivityLogger>();

            return container;
        }

        [TestMethod]
        [TestCategory("Azure")]
        // NOTE: To run this test you must have installed the Azure Storage Emulator. 
        // You can download it here: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
        // The test will automatically start the emulator.
        public async Task AzureStorageUnCompressedTest()
        {
            bool exceptionHappened = false;

            await RunTestCase(false, LargeMessageMode.Trim, QueueKind.AzureQueue);
            await RunTestCase(false, LargeMessageMode.Discard, QueueKind.AzureQueue);

            //expect an exception
            try
            {
                await RunTestCase(false, LargeMessageMode.Error, QueueKind.AzureQueue);
            }
            catch
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened, "Failed to throw exception on large compressed message");
        }


        [TestMethod]
        [TestCategory("Azure")]
        // NOTE: To run this test you must have installed the Azure Storage Emulator. 
        // You can download it here: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
        // The test will automatically start the emulator.
        public async Task AzureStorageCompressedTest()
        {
            bool exceptionHappened = false;

            await RunTestCase(true, LargeMessageMode.Trim, QueueKind.AzureQueue);
            await RunTestCase(true, LargeMessageMode.Discard, QueueKind.AzureQueue);

            //expect an exception
            try
            {
                await RunTestCase(true, LargeMessageMode.Error, QueueKind.AzureQueue);
            }
            catch
            {
                exceptionHappened = true;

            }

            Assert.IsTrue(exceptionHappened, "Failed to throw exception on large compressed message");
        }
    }
}