using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class DocumentDbDeleter
    {
        private readonly DocumentClient documentClient;
        private readonly string databaseId;

        public DocumentDbDeleter(Uri serviceEndpoint, string authKey, string databaseId = "botdb")
        {
            documentClient = new DocumentClient(serviceEndpoint, authKey);
            SetField.NotNull(out this.databaseId, nameof(databaseId), databaseId);
        }

        public async Task DeleteDatabaseIfExists()
        {
            try
            {
                await documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
                await documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    //Database already non existent, nothing to do here.
                }
                else
                {
                    throw;
                }
            }
        }
    }
}