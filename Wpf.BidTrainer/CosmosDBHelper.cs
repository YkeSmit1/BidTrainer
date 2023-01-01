using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public static class CosmosDbHelper
    {
        public static async Task<IEnumerable<Account>> GetAllAccounts()
        {
            var accounts = new List<Account>();

            var queryDefinition = new QueryDefinition("select * from c");
            using var query = GetContainer().GetItemQueryIterator<Account>(queryDefinition);
            {
                while (query.HasMoreResults)
                {
                    accounts.AddRange(await query.ReadNextAsync());
                }
            }
            return accounts;
        }

        public static async Task<Account?> GetAccount(string username)
        {
            var querydefinition = new QueryDefinition($"select * from c where c.username = '{username}'");
            using var query = GetContainer().GetItemQueryIterator<Account>(querydefinition);
            while (query.HasMoreResults)
            {
                var account = await query.ReadNextAsync();
                if (account.Count != 0)
                    return account.First();
            }

            return null;
        }

        public static async Task InsertAccount(Account account)
        {
            await GetContainer().CreateItemAsync(account);
        }

        public static async Task UpdateAccount(Account account)
        {
            await GetContainer().UpsertItemAsync(account);
        }

        private static Container GetContainer()
        {
            var client = new CosmosClient(Constants.EndpointUri, Constants.PrimaryKey,
                new CosmosClientOptions { ApplicationRegion = Regions.WestEurope, });
            return client.GetContainer(Constants.DatabaseName, Constants.CollectionName);
        }

    }
}
