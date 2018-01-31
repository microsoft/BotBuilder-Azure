using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder.Azure
{ 
    /// <summary>
    /// This bridge class should be used during a migration of data from one BotDataStore to another
    /// </summary>
    /// <remarks>
    /// It works by reading from both source and target stores and only writing data to the new store.
    /// This allows live code to continue to work while a background job is migrating data from the source store to the target store.
    /// To use: deploy your bot using this dual data store while a background job is migrating the data from the
    /// source store to the target store. When the background migration job is done you can deploy your bot using the target store
    /// 
    /// NOTE: migration job should ignore precondition failures on write, because if the record already exists in the new store it was migrated 
    /// dynamically by the use of this class
    /// </remarks>
    public class BotDataStoreBridge : IBotDataStore<BotData>
    {
        private IBotDataStore<BotData> sourceStore;
        private IBotDataStore<BotData> targetStore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceStore"></param>
        /// <param name="targetStore"></param>
        /// <param name="deleteOnSuccess">if true, a successful write to the target store will remove from the source</param>
        public BotDataStoreBridge(IBotDataStore<BotData> sourceStore, IBotDataStore<BotData> targetStore)
        {
            this.sourceStore = sourceStore ?? throw new ArgumentNullException("sourceStore");
            this.targetStore = targetStore ?? throw new ArgumentNullException("targetStore");
        }

        public async Task<BotData> LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            // read from both in parallel
            var sourceStoreReadTask = this.sourceStore.LoadAsync(key, botStoreType, cancellationToken);
            var targetStoreReadTask = this.targetStore.LoadAsync(key, botStoreType, cancellationToken);
            await Task.WhenAll(sourceStoreReadTask, targetStoreReadTask).ConfigureAwait(false);

            // if record isn't in targetStore yet, return sourceStore record
            if (String.IsNullOrEmpty(targetStoreReadTask.Result.ETag) || targetStoreReadTask.Result.ETag == "*")
            {
                // but with wildcard etag so save will create new record in target store
                sourceStoreReadTask.Result.ETag = "*";
                return sourceStoreReadTask.Result;
            }
            return targetStoreReadTask.Result;
        }

        public async Task SaveAsync(IAddress key, BotStoreType botStoreType, BotData data, CancellationToken cancellationToken)
        {
            // Always save to target store
            await this.targetStore.SaveAsync(key, botStoreType, data, cancellationToken);
        }

        public Task<bool> FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // always flush to target store (since we are only writing to target store)
            return this.targetStore.FlushAsync(key, cancellationToken);
        }
    }
}
