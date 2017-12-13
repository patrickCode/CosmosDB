using System.Collections.Generic;
using Microsoft.Azure.CosmosDB.Table;

namespace Table.Data
{
    public class TableResponse<TElement> where TElement: TableEntity
    {
        public List<TElement> Results { get; }
        public string ContinuationToken { get; }
        public TableResponse(List<TElement> results, string continutationToken)
        {
            Results = results;
            ContinuationToken = continutationToken;
        }
    }
}