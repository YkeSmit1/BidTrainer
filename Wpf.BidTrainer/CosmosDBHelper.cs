using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public class CosmosDBHelper
    {
        private static readonly DocumentClient client = new DocumentClient(new Uri(Constants.EndpointUri), Constants.PrimaryKey);
        private static readonly Uri collectionLink = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.CollectionName);
        private static readonly FeedOptions feedOptions = new FeedOptions { EnableCrossPartitionQuery = true };
        private static readonly IOrderedQueryable<Account> queryable = client.CreateDocumentQuery<Account>(collectionLink, feedOptions);

        public static async Task<IEnumerable<Account>> GetAllAccounts()
        {
            var accounts = new List<Account>();
            var query = queryable.AsDocumentQuery();
            while (query.HasMoreResults)
            {
                accounts.AddRange(await query.ExecuteNextAsync<Account>());
            }
            return accounts;
        }

        public static async Task<Account?> GetAccount(string username)
        {
            var account = await queryable.Where(x => x.username == username).AsDocumentQuery().ExecuteNextAsync<Account>();
            return account.FirstOrDefault();
        }

        public static async Task InsertAccount(Account account)
        {
            await client.CreateDocumentAsync(collectionLink, account);
        }

        public static async Task UpdateAccount(Account account)
        {
            await client.UpsertDocumentAsync(collectionLink, account);
        }
    }
}
