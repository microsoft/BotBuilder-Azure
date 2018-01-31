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
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// <see cref="IBotDataStore{T}"/> Implementation using Azure SQL 
    /// </summary>
    public class SqlBotDataStore : IBotDataStore<BotData>
    {
        string _connectionString { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="IBotDataStore{T}"/> that uses the azure sql storage.
        /// </summary>
        /// <param name="connectionString">The storage connection string.</param>
        public SqlBotDataStore(string connectionString)
        {
            _connectionString = connectionString;
        }
        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            using (var context = new SqlBotDataContext(_connectionString))
            {
                try
                {
                    SqlBotDataEntity entity = await SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);

                    if (entity == null)
                        // empty record ready to be saved
                        return new BotData(eTag: String.Empty, data: null);

                    // return botdata 
                    return new BotData(entity.ETag, entity.GetData());
                }
                catch (System.Data.SqlClient.SqlException err)
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, err.Message);
                }
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            SqlBotDataEntity entity = new SqlBotDataEntity(botStoreType, key.BotId, key.ChannelId, key.ConversationId, key.UserId, botData.Data)
            {
                ETag = botData.ETag,
                ServiceUrl = key.ServiceUrl
            };

            using (var context = new SqlBotDataContext(_connectionString))
            {
                try
                {
                    if (string.IsNullOrEmpty(botData.ETag))
                    {
                        context.BotData.Add(entity);
                    }
                    else if (entity.ETag == "*")
                    {
                        var foundData = await SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);
                        if (botData.Data != null)
                        {
                            if (foundData == null)
                                context.BotData.Add(entity);
                            else
                            {
                                foundData.Data = entity.Data;
                                foundData.ServiceUrl = entity.ServiceUrl;
                            }
                        }
                        else
                        {
                            if (foundData != null)
                                context.BotData.Remove(foundData);
                        }
                    }
                    else
                    {
                        var foundData = await SqlBotDataEntity.GetSqlBotDataEntity(key, botStoreType, context);
                        if (botData.Data != null)
                        {
                            if (foundData == null)
                                context.BotData.Add(entity);
                            else
                            {
                                foundData.Data = entity.Data;
                                foundData.ServiceUrl = entity.ServiceUrl;
                                foundData.ETag = entity.ETag;
                            }
                        }
                        else
                        {
                            if (foundData != null)
                                context.BotData.Remove(foundData);
                        }
                    }
                    await context.SaveChangesAsync();
                }
                catch (System.Data.SqlClient.SqlException err)
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, err.Message);
                }
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }        
    }

    internal class SqlBotDataContext : DbContext
    {
        public SqlBotDataContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer<SqlBotDataContext>(null);
        }

        /// <summary>
        /// Throw if the database or SqlBotDataEntities table have not been created.
        /// </summary>
        static internal void AssertDatabaseReady()
        {
            var connectionString = Utils.GetAppSetting(AppSettingKeys.SqlServerConnectionString);
            using (var context = new SqlBotDataContext(connectionString))
            {
                if (!context.Database.Exists())
                    throw new ArgumentException("The sql database defined in the connection has not been created. See https://github.com/Microsoft/BotBuilder-Azure/tree/master/CSharp");

                if (context.Database.SqlQuery<int>(@"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SqlBotDataEntities') 
                                                                    SELECT 1
                                                                ELSE
                                                                    SELECT 0").SingleOrDefault() != 1)
                    throw new ArgumentException("The SqlBotDataEntities table has not been created in the database. See https://github.com/Microsoft/BotBuilder-Azure/tree/master/CSharp");
            }
        }

        public DbSet<SqlBotDataEntity> BotData { get; set; }
    }

    internal class SqlBotDataEntity : IAddress
    {
        private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
        internal SqlBotDataEntity() { Timestamp = DateTimeOffset.UtcNow; }
        internal SqlBotDataEntity(BotStoreType botStoreType, string botId, string channelId, string conversationId, string userId, object data)
        {
            this.BotStoreType = botStoreType;
            this.BotId = botId;
            this.ChannelId = channelId;
            this.ConversationId = conversationId;
            this.UserId = userId;
            this.Data = Serialize(data);
            Timestamp = DateTimeOffset.UtcNow;
        }


        #region Fields

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Index("idxStoreChannelUser", 1)]
        [Index("idxStoreChannelConversation", 1)]
        [Index("idxStoreChannelConversationUser", 1)]
        public BotStoreType BotStoreType { get; set; }
        public string BotId { get; set; }
        [Index("idxStoreChannelConversation", 2)]
        [Index("idxStoreChannelUser", 2)]
        [Index("idxStoreChannelConversationUser", 2)]
        [MaxLength(200)]
        public string ChannelId { get; set; }
        [Index("idxStoreChannelConversation", 3)]
        [Index("idxStoreChannelConversationUser", 3)]
        [MaxLength(200)]
        public string ConversationId { get; set; }
        [Index("idxStoreChannelUser", 3)]
        [Index("idxStoreChannelConversationUser", 4)]
        [MaxLength(200)]
        public string UserId { get; set; }
        public byte[] Data { get; set; }
        public string ETag { get; set; }
        public string ServiceUrl { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }

        #endregion Fields

        #region Methods

        private static byte[] Serialize(object data)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            using (var streamWriter = new StreamWriter(stream))
            {
                var serializedJSon = JsonConvert.SerializeObject(data, serializationSettings);
                streamWriter.Write(serializedJSon);
                streamWriter.Close();
                stream.Close();
                return cmpStream.ToArray();
            }
        }

        private static object Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gz))
            {
                return JsonConvert.DeserializeObject(streamReader.ReadToEnd());
            }
        }

        internal ObjectT GetData<ObjectT>()
        {
            return ((JObject)Deserialize(this.Data)).ToObject<ObjectT>();
        }

        internal object GetData()
        {
            return Deserialize(this.Data);
        }
        internal static async Task<SqlBotDataEntity> GetSqlBotDataEntity(IAddress key, BotStoreType botStoreType, SqlBotDataContext context)
        {
            SqlBotDataEntity entity = null;
            var query = context.BotData.OrderByDescending(d => d.Timestamp);
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    entity = await query.FirstOrDefaultAsync(d => d.BotStoreType == botStoreType
                                                    && d.ChannelId == key.ChannelId
                                                    && d.ConversationId == key.ConversationId);
                    break;
                case BotStoreType.BotUserData:
                    entity = await query.FirstOrDefaultAsync(d => d.BotStoreType == botStoreType
                                                    && d.ChannelId == key.ChannelId
                                                    && d.UserId == key.UserId);
                    break;
                case BotStoreType.BotPrivateConversationData:
                    entity = await query.FirstOrDefaultAsync(d => d.BotStoreType == botStoreType
                                                    && d.ChannelId == key.ChannelId
                                                    && d.ConversationId == key.ConversationId
                                                    && d.UserId == key.UserId);
                    break;
                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }

            return entity;
        }
        #endregion
    }

}