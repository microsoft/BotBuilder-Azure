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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Net;
using System.Threading;
using System.Web;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure
{

    /// <summary>
    /// <see cref="IBotDataStore{T}"/> Implementation using Azure Storage Table,
    /// </summary>
    /// <notes>
    /// The original TableBotDataStore put all conversation data in 1 partition in 1 table which severely limits the scalability. 
    /// This TableBotDataStore2 class changes to use multiple tables, one tableName="{BotId}{ChannelId}[Conversation|User]" 
    /// It then uses the UserId and ConversationId as the PartitionKeys within those tables to achieve much higher scalability charateristics
    /// </notes>
    public class TableBotDataStore2 : IBotDataStore<BotData>
    {
        private static Dictionary<string, CloudTable> tables = new Dictionary<string, CloudTable>();
        private CloudTableClient tableClient;

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure table storage.
        /// </summary>
        /// <param name="connectionString">The storage connection string.</param>
        /// <param name="tableName">The name of table.</param>
        public TableBotDataStore2(string connectionString)
            : this(CloudStorageAccount.Parse(connectionString))
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure table storage.
        /// </summary>
        /// <param name="storageAccount">The storage account.</param>
        /// <param name="tableName">The name of table.</param>
        public TableBotDataStore2(CloudStorageAccount storageAccount)
        {
            tableClient = storageAccount.CreateCloudTableClient();
        }

        public IEnumerable<CloudTable> Tables { get { return tables.Values; } }

        private CloudTable GetTable(IAddress key, BotStoreType botStoreType)
        {
            string tableName;
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                case BotStoreType.BotPrivateConversationData:
                    tableName = $"{key.BotId}{key.ChannelId}Conversations".SanitizeForAzureKeys();
                    break;
                case BotStoreType.BotUserData:
                    tableName = $"{key.BotId}{key.ChannelId}Users".SanitizeForAzureKeys();
                    break;
                default:
                    throw new ArgumentException("BotStoreType is unknown");
            }

            lock (tables)
            {
                CloudTable table;
                if (tables.TryGetValue(tableName, out table))
                    return table;

                table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();
                tables.Add(tableName, table);
                return table;
            }
        }

        internal static EntityKey GetEntityKey(IAddress key, BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotUserData:
                    return new EntityKey(key.UserId, "user");

                case BotStoreType.BotConversationData:
                    return new EntityKey(key.ConversationId, "conversation");

                case BotStoreType.BotPrivateConversationData:
                    return new EntityKey(key.ConversationId, key.UserId);

                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }


        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            var entityKey = GetEntityKey(key, botStoreType);
            var table = this.GetTable(key, botStoreType);
            try
            {
                var result = await table.ExecuteAsync(TableOperation.Retrieve<BotDataEntity>(entityKey.PartitionKey, entityKey.RowKey));
                BotDataEntity entity = (BotDataEntity)result.Result;
                if (entity == null)
                    // empty record ready to be saved
                    return new BotData(eTag: String.Empty, data: null);

                // return botdata 
                return new BotData(entity.ETag, entity.GetData());
            }
            catch (StorageException err)
            {
                throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            var table = this.GetTable(key, botStoreType);
            var entityKey = GetEntityKey(key, botStoreType);
            BotDataEntity entity = new BotDataEntity(key.BotId, key.ChannelId, key.ConversationId, key.UserId, botData.Data)
            {
                ETag = botData.ETag
            };
            entity.PartitionKey = entityKey.PartitionKey;
            entity.RowKey = entityKey.RowKey;
            try
            {
                if (String.IsNullOrEmpty(entity.ETag))
                    await table.ExecuteAsync(TableOperation.Insert(entity));
                else if (entity.ETag == "*")
                {
                    if (botData.Data != null)
                        await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
                    else
                        await table.ExecuteAsync(TableOperation.Delete(entity));
                }
                else
                {
                    if (botData.Data != null)
                        await table.ExecuteAsync(TableOperation.Replace(entity));
                    else
                        await table.ExecuteAsync(TableOperation.Delete(entity));
                }
            }
            catch (StorageException err)
            {
                if ((HttpStatusCode)err.RequestInformation.HttpStatusCode == HttpStatusCode.Conflict)
                    throw new HttpException((int)HttpStatusCode.PreconditionFailed, err.RequestInformation.HttpStatusMessage);

                throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }

    }
}
