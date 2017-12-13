using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.Table;

namespace Table.Data
{
    public interface ITableRepository<TElement> where TElement : TableEntity, new()
    {
        Task<TElement> GetAsync(string partitionKey, string rowKey, string trackingId = null);
        Task InsertInBatchAsync(List<TElement> entities, string trackingId = null);
        Task<TElement> InsertOrUpdateAsync(TElement entity, string trackingId = null);
        Task<TableResponse<TElement>> QueryAsync(List<QueryParameter> queryParameters, string continuationToken = null, string trackingId = null);
        Task RemoveAsync(TElement entity, string trackingId = null);
    }
}