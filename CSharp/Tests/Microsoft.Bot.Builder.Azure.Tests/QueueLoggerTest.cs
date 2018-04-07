using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
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
        #region SupportingCode
        async Task RunTestCase(bool compressed, LargeMessageMode handlingMode, QueueKind kind)
        {
            IContainer container;
            IQueueReader reader;
            int batchCount;
            var queueSettings = new QueueLoggerSettings
            {
                CompressMessage = compressed,
                OverflowHanding = handlingMode
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

        private static IContainer RegisterServiceBusContainer(out IActivityLogger logger, QueueLoggerSettings queueSettings)
        {
            var queueName = "mytestqueue";
            ///
            ///   Service bus connection string looks like this:
            ///   Endpoint=sb://myservicebusmodule.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=m0nmPm2T4JFIAOs8G2vpJrnz9QKUp1HvZq6baRMIrVs=
            /// 
            var connectionstring = "<enter service bus connection here>";

            if (connectionstring == "<enter service bus connection here>")
                throw new ArgumentException("need to provide a service bus connection string");

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
            int countExceptions  = 0 ;

            for (var i = 0; i < activities.Count; ++i)
            {
                try
                {
                    await logger.LogAsync(activities[i]);
    
                }
                catch (Exception e)
                {
                    countExceptions++;
                    //make sure that we not hiding errors...
                    if (loggerSettings.OverflowHanding != LargeMessageMode.Error)
                        throw e;
                }
            }

            List<Activity> completelist = new List<Activity>();
            List<Activity> readActivities = new List<Activity>() { new Activity()};

            while (readActivities.Count != 0)
            { 
                //now read all messages, using reader
                readActivities = await reader.ReadBatchAsync(batchCount, TimeSpan.FromSeconds(1));
                //give it some time to catch up, if the queue is muti-tennant it will take time for messages to arrive
                Thread.Sleep(2000);
                completelist.AddRange(readActivities);
            }

            //test validation
            if (logger is AzureQueueActivityLogger)
            {
                //make sure we got the same number we sent in.
                if (loggerSettings.OverflowHanding == LargeMessageMode.Trim)
                    Assert.AreEqual(activities.Count, completelist.Count);
                else if (loggerSettings.OverflowHanding == LargeMessageMode.Discard)
                {
                    if (loggerSettings.CompressMessage)
                    { 
                        //here we expect that one messages will be dropped as after compression it is too large in size.
                        Assert.AreEqual(activities.Count, completelist.Count + 1);
                    }
                    else
                    {
                        //here we expect that two messages will be dropped as they are too large in size.
                        Assert.AreEqual(activities.Count, completelist.Count + 2);
                    }
                }
                else if (loggerSettings.OverflowHanding == LargeMessageMode.Error)
                {
                    //here we expect that two messages to generate errors as they are too large in size.
                    Assert.AreEqual(activities.Count, completelist.Count + countExceptions);
                }
            }
            else
            {
                // these messages easily pass so count is good.
                Assert.AreEqual(activities.Count, completelist.Count);
            }
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

        #endregion

        [TestMethod]
        [TestCategory("Azure")]
        // NOTE: To run this test you must have installed the Azure Storage Emulator. 
        // You can download it here: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
        // The test will automatically start the emulator.
        public async Task AzureStorageUnCompressedTest()
        {
            await RunTestCase(false, LargeMessageMode.Trim, QueueKind.AzureQueue);
            await RunTestCase(false, LargeMessageMode.Discard, QueueKind.AzureQueue);
            await RunTestCase(false, LargeMessageMode.Error, QueueKind.AzureQueue);
        }

        [TestMethod]
        [TestCategory("Azure")]
        // NOTE: To run this test you must have installed the Azure Storage Emulator. 
        // You can download it here: https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409
        // The test will automatically start the emulator.
        public async Task AzureStorageCompressedTest()
        {
            await RunTestCase(true, LargeMessageMode.Trim, QueueKind.AzureQueue);
            await RunTestCase(true, LargeMessageMode.Discard, QueueKind.AzureQueue);
            await RunTestCase(true, LargeMessageMode.Error, QueueKind.AzureQueue);
        }

        [Ignore]
        [TestMethod]
        [TestCategory("Azure")]
        public async Task ServiceBusTestUnCompressed()
        {
            await RunTestCase(false, LargeMessageMode.Trim, QueueKind.ServiceBus);
            await RunTestCase(false, LargeMessageMode.Discard, QueueKind.ServiceBus);
            await RunTestCase(false, LargeMessageMode.Error, QueueKind.ServiceBus);
        }

        [Ignore]
        [TestMethod]
        [TestCategory("Azure")]
        public async Task ServiceBusTestCompressed()
        {
            await RunTestCase(true, LargeMessageMode.Trim, QueueKind.ServiceBus);
            await RunTestCase(true, LargeMessageMode.Discard, QueueKind.ServiceBus);
            await RunTestCase(true, LargeMessageMode.Error, QueueKind.ServiceBus);
        }
    }
}