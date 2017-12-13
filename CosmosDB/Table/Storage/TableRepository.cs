using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using System.Collections.Generic;
using Microsoft.Azure.CosmosDB.Table;

namespace Table.Data
{
    public class TableRepository<TElement> : ITableRepository<TElement> where TElement: TableEntity, new()
    {
        private readonly string _tableName;
        private readonly ICloudStorageContext _cloudStorageContext;
        private readonly CloudTable _table;

        public TableRepository(string tableName, ICloudStorageContext cloudStorageContext) 
        {
            _tableName = tableName;
            _cloudStorageContext = cloudStorageContext;
            _table = _cloudStorageContext.CreateTable(_tableName).Result;
        }

        public async Task<TElement> GetAsync(string partitionKey, string rowKey, string trackingId = null)
        {
            var retreiveOperation = TableOperation.Retrieve<TElement>(partitionKey, rowKey);
            var result = await ExecuteTableCommandAsync(retreiveOperation, trackingId);
            return result.Result as TElement;
        }

        public async Task<TableResponse<TElement>> QueryAsync(List<QueryParameter> queryParameters, string continuationToken = null, string trackingId = null)
        {
            var query = new TableQuery<TElement>()
                .Where(CreateQueryFilter(queryParameters));

            TableContinuationToken tableContinuationToken = null;
            if (!(string.IsNullOrEmpty(continuationToken)))
            {
                tableContinuationToken = new TableContinuationToken();
                var xmlTokenReader = XmlReader.Create(new StringReader(continuationToken));
                tableContinuationToken.ReadXml(xmlTokenReader);
            }

            var result = await _table.ExecuteQuerySegmentedAsync(query, tableContinuationToken);

            var token = result.ContinuationToken;
            string tokenStr = null;
            if (token != null)
            {
                var stringTokenWriter = new StringWriter();
                var xmlTokenWriter = new XmlTextWriter(stringTokenWriter);
                token.WriteXml(xmlTokenWriter);
                tokenStr = stringTokenWriter.ToString();
            }

            var response = new TableResponse<TElement>(result.Results, tokenStr);
            return response;
        }

        public async Task<TElement> InsertOrUpdateAsync(TElement entity, string trackingId = null)
        {
            var insertOperation = TableOperation.InsertOrMerge(entity);
            var result = await ExecuteTableCommandAsync(insertOperation, trackingId);
            return result.Result as TElement;
        }

        public async Task RemoveAsync(TElement entity, string trackingId = null)
        {
            var deletionOperation = TableOperation.Delete(entity);
            await ExecuteTableCommandAsync(deletionOperation, trackingId);
        }

        public Task InsertInBatchAsync(List<TElement> entities, string trackingId = null)
        {   
            var batchGroupsByPartitionKey = entities.GroupBy((entity) => entity.PartitionKey);
            Parallel.ForEach(batchGroupsByPartitionKey, async (batch) =>
            {
                await InsertCommonElementsInBatch(batch.ToList(), trackingId);
            });
            return Task.CompletedTask;
        }

        private Task InsertCommonElementsInBatch(List<TElement> entities, string trackingId = null)
        {
            var indexedEntitites = entities.Select((entity, index) => new
            {
                Index = index,
                Entity = entity
            });
            var batchGroupsByIndex = indexedEntitites.GroupBy(entity => entity.Index / 100);
            Parallel.ForEach(batchGroupsByIndex, async (batch) =>
            {
                var tableBatchOperation = new TableBatchOperation();
                var singleBatchEntities = batch.Select(e => e.Entity).ToList();
                singleBatchEntities.ForEach(entity => tableBatchOperation.InsertOrMerge(entity));

                var requestOptions = _cloudStorageContext.GetDefaultRequestOption();
                var operationContext = GetDefaultOperationContext(trackingId);
                await _table.ExecuteBatchAsync(tableBatchOperation, requestOptions, operationContext);
            });
            return Task.CompletedTask;
        }

        private async Task<TableResult> ExecuteTableCommandAsync(TableOperation operation, string trackingId = null)
        {
            var requestOptions = _cloudStorageContext.GetDefaultRequestOption();
            var operationContext = GetDefaultOperationContext(trackingId);
            var result = await _table.ExecuteAsync(operation, requestOptions, operationContext);
            return result;
        }

        private OperationContext GetDefaultOperationContext(string trackingId = null)
        {
            var operationContext = new OperationContext()
            {
                ClientRequestID = string.IsNullOrEmpty(trackingId) ? Guid.NewGuid().ToString() : trackingId,
                StartTime = DateTime.UtcNow,
                LogLevel = LogLevel.Verbose
            };
            return operationContext;
        }

        private string CreateQueryFilter(List<QueryParameter> queryParameters, string generatedQuery = null, int currentParameterPosition = 0)
        {
            var property = queryParameters[currentParameterPosition].Propery;
            var operation = queryParameters[currentParameterPosition].Operation;
            var value = queryParameters[currentParameterPosition].Value;

            string query = TableQuery.GenerateFilterCondition(property, operation, value);
            if (generatedQuery != null)
            {
                query = TableQuery.CombineFilters(generatedQuery, TableOperators.And, query);
            }

            if (queryParameters.Count == currentParameterPosition + 1)
                return query;

            currentParameterPosition++;
            return CreateQueryFilter(queryParameters, query, currentParameterPosition);
        }
    }
}