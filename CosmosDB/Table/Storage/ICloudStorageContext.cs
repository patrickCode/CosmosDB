using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.Table;

namespace Table.Data
{
    public interface ICloudStorageContext
    {
        Task<CloudTable> CreateTable(string tableName);
        TableRequestOptions GetDefaultRequestOption();
    }
}