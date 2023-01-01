using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wpf.BidTrainer
{
    public static class CosmosDbHelper
    {
        private static readonly CosmosClient Client = new(Constants.EndpointUri, Constants.PrimaryKey, new CosmosClientOptions { ApplicationRegion = Regions.WestEurope, });
        private static readonly Container Container = Client.GetContainer(Constants.DatabaseName, Constants.CollectionName);

        public static async Task<IEnumerable<Account>> GetAllAccounts()
        {
            var queryDefinition = new QueryDefinition("select * from c");
            using var query = Container.GetItemQueryIterator<Account>(queryDefinition);
            return await query.ReadNextAsync();
        }

        public static async Task<Account?> GetAccount(string username)
        {
            var queryDefinition = new QueryDefinition($"select * from c where c.username = '{username}'");
            using var query = Container.GetItemQueryIterator<Account>(queryDefinition);
            var account = await query.ReadNextAsync();
            return account.FirstOrDefault();
        }

        public static async Task InsertAccount(Account account)
        {
            await Container.CreateItemAsync(account);
        }

        public static async Task UpdateAccount(Account account)
        {
            await Container.UpsertItemAsync(account);
        }
    }
}
