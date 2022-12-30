using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public static class CosmosDbHelper
    {
        private static readonly DocumentClient Client = new(new Uri(Constants.EndpointUri), Constants.PrimaryKey);
        private static readonly Uri CollectionLink = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.CollectionName);
        private static readonly FeedOptions FeedOptions = new() { EnableCrossPartitionQuery = true };

        public static async Task<IEnumerable<Account>> GetAllAccounts()
        {
            var accounts = new List<Account>();
            var query = Client.CreateDocumentQuery<Account>(CollectionLink, FeedOptions).AsDocumentQuery();
            while (query.HasMoreResults)
            {
                accounts.AddRange(await query.ExecuteNextAsync<Account>());
            }
            return accounts;
        }

        public static async Task<Account?> GetAccount(string username)
        {
            var account = await Client.CreateDocumentQuery<Account>(CollectionLink, FeedOptions).
                Where(x => x.username == username).AsDocumentQuery().ExecuteNextAsync<Account>();
            return account.FirstOrDefault();
        }

        public static async Task InsertAccount(Account account)
        {
            await Client.CreateDocumentAsync(CollectionLink, account);
        }

        public static async Task UpdateAccount(Account account)
        {
            await Client.UpsertDocumentAsync(CollectionLink, account);
        }
    }
}
