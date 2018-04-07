using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Azure.Documents.Client;
using System.Web;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class TestAddress : IAddress
    {
        public string BotId { get; set; }

        public string ChannelId { get; set; }

        public string UserId { get; set; }

        public string ConversationId { get; set; }

        public string ServiceUrl { get; set; }
    }


    public class TestData
    {
        public string Store { get; set; }
        public int Value { get; set; }
        public string Value2 { get; set; }
    }

    [TestClass]
    public class BotDataStoreMigratorTest
    {
        private static IBotDataStore<BotData> GetSourceStore()
        {
            // uncomment to test using stateAPI as source store
            //return new ConnectorStore(new StateClient(new MicrosoftAppCredentials("9871029d-5717-4b02-8870-8184ad3be33e", "K$i3/8Jho}+d1pqP")));
            return new InMemoryDataStore();
        }

        private static async Task<IBotDataStore<BotData>> CreateDocumentDbStore(string botdb = "botdb")
        {
            var docClient = new DocumentClient(new Uri("https://localhost:8081"), "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            var database = docClient.CreateDatabaseQuery().Where(db => db.Id == botdb).ToArray().FirstOrDefault();
            if (database != null)
                await docClient.DeleteDatabaseAsync(database.SelfLink);
            var targetStore = new DocumentDbBotDataStore(docClient);
            return targetStore;
        }


        private static async Task<IBotDataStore<BotData>> CreateTableStore(string tableName = "target")
        {
            var targetStore = new TableBotDataStore("UseDevelopmentStorage=true", tableName);
            await targetStore.Table.DeleteIfExistsAsync();
            await targetStore.Table.CreateIfNotExistsAsync();
            return targetStore;
        }

        private static IAddress CreateAddress(int i)
        {
            var address = new TestAddress()
            {
                BotId = "MyFunctionBot",
                ChannelId = "test",
                ConversationId = (i % 2).ToString(),
                UserId = "User" + (i % 10).ToString()
            };
            return address;
        }


        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task Table_TestBotConversationDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateTableStore();
            await TestStoreType(BotStoreType.BotConversationData, 10, sourceStore, targetStore);
        }

        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task Table_TestBotPrivateConversationDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateTableStore();
            await TestStoreType(BotStoreType.BotPrivateConversationData, 10, sourceStore, targetStore);
        }

        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task Table_TestBotUserDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateTableStore();
            await TestStoreType(BotStoreType.BotUserData, 10, sourceStore, targetStore);
        }

        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task DocumentDB_TestBotConversationDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateDocumentDbStore();
            await TestStoreType(BotStoreType.BotConversationData, 10, sourceStore, targetStore);
        }

        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task DocumentDB_TestBotPrivateConversationDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateDocumentDbStore();
            await TestStoreType(BotStoreType.BotPrivateConversationData, 10, sourceStore, targetStore);
        }

        [TestMethod]
        [TestCategory("BotDataStoreBridge")]
        public async Task DocumentDB_TestBotUserDataMigration()
        {
            var sourceStore = GetSourceStore();
            var targetStore = await CreateDocumentDbStore();
            await TestStoreType(BotStoreType.BotUserData, 10, sourceStore, targetStore);
        }
        
        private static async Task TestStoreType(BotStoreType storeType, int iterations, IBotDataStore<BotData> sourceStore, IBotDataStore<BotData> targetStore)
        {
            Random rnd = new Random();

            // fill source store
            for (int i = 0; i < iterations; i++)
                await sourceStore.SaveAsync(CreateAddress(i), storeType, new BotData(eTag: "*", data: new TestData() { Store = "source", Value = i }), CancellationToken.None);

            var bridgeStore = (IBotDataStore<BotData>)new BotDataStoreBridge(sourceStore, targetStore);

            for (int i = 0; i < iterations; i++)
            {
                // every object should be readable from source 
                var result = await bridgeStore.LoadAsync(CreateAddress(i), storeType, CancellationToken.None);
                Assert.IsNotNull(result.Data);
                Assert.IsTrue(!String.IsNullOrEmpty(result.ETag));

                // every object should not be in target
                var result2 = await targetStore.LoadAsync(CreateAddress(i), storeType, CancellationToken.None);
                Assert.IsNull(result2.Data);
                Assert.IsTrue(String.IsNullOrEmpty(result2.ETag));
            }

            // Save new object should work
            try
            {
                await bridgeStore.SaveAsync(new TestAddress()
                {
                    BotId = "MyFunctionBot",
                    ChannelId = "test",
                    ConversationId = "ACONVERATION",
                    UserId = "AUSER"
                }, storeType, new BotData(eTag: "*", data: new TestData() { Store = "target", Value = int.MaxValue }), CancellationToken.None);
            }
            catch (Exception err)
            {
                Assert.Fail($"Failed to write new record to bridge store:{err.Message}\n{err.ToString()}");
            }

            for (int i = 0; i < iterations; i++)
            {
                var address = CreateAddress(i);
                // Every object should be still be loadable
                BotData botData = await bridgeStore.LoadAsync(address, storeType, CancellationToken.None);

                // change object and save 
                var data = ((JObject)botData.Data).ToObject<TestData>();
                data.Store = "target";
                botData.Data = data;

                // Every change should be saved to targetStore
                await bridgeStore.SaveAsync(address, storeType, botData, CancellationToken.None);
            }

            // verify write with ETag and precondition failure 
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    var address = CreateAddress(i);
                    var result = await bridgeStore.LoadAsync(address, storeType, CancellationToken.None);
                    // set Value2 to good
                    await bridgeStore.SaveAsync(address, storeType, new BotData(eTag: result.ETag, data: new TestData() { Store = "target", Value = i, Value2 = "good" }), CancellationToken.None);
                    
                    // bad attempt to set Value2 to bad
                    await bridgeStore.SaveAsync(address, storeType, new BotData(eTag: result.ETag, data: new TestData() { Store = "target", Value = i, Value2 = "bad" }), CancellationToken.None);
                    Assert.Fail("Should throw HttpException with precondition failure");
                }
                catch (HttpException err)
                {
                    Assert.AreEqual(412, err.GetHttpCode(), "Expected precondition failure");
                }
            }

            // every object should now be in targetStore 
            for (int i = 0; i < iterations; i++)
            {
                var address = CreateAddress(i);

                BotData result1 = await sourceStore.LoadAsync(address, storeType, CancellationToken.None);
                BotData result2 = await targetStore.LoadAsync(address, storeType, CancellationToken.None);

                var data1 = ((JObject)result1.Data).ToObject<TestData>();
                var data2 = ((JObject)result2.Data).ToObject<TestData>();

                Assert.IsNotNull(result1.Data, "Every object should be loadable from source store");
                Assert.IsNotNull(result2.Data, "Every object should be loadable from target store");

                Assert.AreEqual(data1.Value, data2.Value, "values should be the same");

                Assert.AreEqual(data1.Store, "source", "source record should be source");
                Assert.AreEqual(data2.Store, "target", "target record should be target");

                Assert.AreEqual(data2.Value2, "good", "target record value2 should be good");

                Assert.AreNotEqual(result1.ETag, result2.ETag, "ETags should be different");
            }
        }
    }
}
