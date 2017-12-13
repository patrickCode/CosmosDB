using Microsoft.Azure.CosmosDB.Table;

namespace Table.Data.Entities
{
    public class UserTableEntity : TableEntity
    {
        //PartitionKey - User Role
        //RowKey - User Id
        public UserTableEntity() { }
        public string UniquePrincipalName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}