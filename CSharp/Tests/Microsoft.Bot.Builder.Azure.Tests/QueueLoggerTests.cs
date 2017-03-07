//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Autofac;
//using Microsoft.Bot.Builder.History;
//using Microsoft.Bot.Builder.Tests;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.WindowsAzure.Storage;

//namespace Microsoft.Bot.Builder.Azure.Tests
//{
//    public class QueueLoggerTests : LoggerTestBase
//    {
//        [TestMethod]
//        [TestCategory("Azure")]
//        public async Task QueueLoggerTest()
//        {
//            var queueName = "myquename";
//            var account = CloudStorageAccount.DevelopmentStorageAccount;
//            account.CreateCloudQueueClient().GetQueueReference(queueName).DeleteIfExists();
//            var builder = new ContainerBuilder();
//            builder.RegisterModule(new QueueActivityModule(account, queueName,null));
//            var container = builder.Build();
//            var logger = container.Resolve<IActivityLogger>();
//            var source = container.Resolve<IActivitySource>();
//            var manager = container.Resolve<IActivityManager>();

//            var activities = GetTestActivityList();

//            for (var i = 0; i < activities.Count; ++i)
//            {
//                await logger.LogAsync(activities[i]);
//            }
//        }
//    }
//}
