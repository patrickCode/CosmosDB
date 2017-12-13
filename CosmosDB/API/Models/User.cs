using Table.Data.Entities;

namespace CosmosDB.Web.Models
{
    public class User
    {
        public User() { }

        public User(string id, string userRole)
        {
            Id = id;
            UserRole = userRole;
        }

        public User(UserTableEntity userTableEntity)
        {
            CreateFromTableEntity(userTableEntity);
        }

        public string Id { get; set; }
        public string UniquePrincipalName { get; set; }
        public string UserRole { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public UserContact Contact { get; set; }

        public UserTableEntity ToUserTableEntity()
        {
            return new UserTableEntity()
            {
                PartitionKey = this.UserRole,
                RowKey = this.Id,
                UniquePrincipalName = this.UniquePrincipalName,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                LastName = this.LastName,
                EmailAddress = this.Contact.EmailAddress,
                PhoneNumber = this.Contact.PhoneNumber
            };
        }

        private void CreateFromTableEntity(UserTableEntity userTableEntity)
        {
            this.Id = userTableEntity.RowKey;
            this.UserRole = userTableEntity.PartitionKey;
            this.UniquePrincipalName = userTableEntity.UniquePrincipalName;
            this.FirstName = userTableEntity.FirstName;
            this.MiddleName = userTableEntity.MiddleName;
            this.LastName = userTableEntity.LastName;
            this.Contact = new UserContact()
            {
                EmailAddress = userTableEntity.EmailAddress,
                PhoneNumber = userTableEntity.PhoneNumber
            };
        }
    }

    public class UserContact
    {
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
    }
}