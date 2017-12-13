using System;
using Table.Data;
using System.Linq;
using Table.Data.Entities;
using CosmosDB.Web.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace CosmosDB.Web.Controllers
{
    [Route("api/users")]
    public class TablesController: Controller
    {
        private readonly ITableRepository<UserTableEntity> _tableRepository;

        public TablesController(ITableRepository<UserTableEntity> tableRepository)
        {
            _tableRepository = tableRepository;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<User> Get([FromRoute]string userId, [FromQuery]string userRole)
        {
            var correlationId = GetCorrelationId();
            var userTableEntity = await _tableRepository.GetAsync(userRole, userId, correlationId);
            return new User(userTableEntity);
        }

        [HttpPost]
        public async Task<User> Create([FromBody] User user)
        {
            user.Id = Guid.NewGuid().ToString();
            var userTableEntity = user.ToUserTableEntity();
            var correlationId = GetCorrelationId();
            var insertedTableEntity = await _tableRepository.InsertOrUpdateAsync(userTableEntity, correlationId);
            return new User(insertedTableEntity);
        }

        [HttpPost]
        [Route("bulk")]
        public async Task<List<string>> Create([FromBody] List<User> users)
        {
            var userIds = new List<string>();
            var userTableEntities = users.Select(user =>
            {
                user.Id = Guid.NewGuid().ToString();
                userIds.Add(user.Id);
                return user.ToUserTableEntity();
            }).ToList();
            
            var correlationId = GetCorrelationId();
            await _tableRepository.InsertInBatchAsync(userTableEntities, correlationId);
            return userIds;
        }

        [HttpPut]
        public async Task<User> Edit([FromBody] User user)
        {
            user.Id = Guid.NewGuid().ToString();
            var userTableEntity = user.ToUserTableEntity();
            var correlationId = GetCorrelationId();
            var insertedTableEntity = await _tableRepository.InsertOrUpdateAsync(userTableEntity, correlationId);
            return new User(insertedTableEntity);
        }

        [HttpGet]
        [Route("query")]
        public async Task<List<User>> SearchByName(string firstName)
        {
            var correlationId = GetCorrelationId();
            var queryParameters = new List<QueryParameter>()
            {
                new QueryParameter("FirstName", "eq", firstName)
            };
            var userQueryResponse = await _tableRepository.QueryAsync(queryParameters, null, correlationId);
            var users = userQueryResponse.Results.Select(userTableEntity => new User(userTableEntity));
            return users.ToList();
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task Delete([FromRoute]string userId, [FromQuery]string userRole)
        {
            var user = new User(userId, userRole);
            var correlationId = GetCorrelationId();
            await _tableRepository.RemoveAsync(user.ToUserTableEntity(), correlationId);
        }

        private string GetCorrelationId()
        {
            string correlationId = string.Empty;
            if (Request.Headers.TryGetValue("x-correlationId", out StringValues correlationValues))
            {
                correlationId = correlationValues.FirstOrDefault();
            }
            correlationId = string.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
            return correlationId;
        }
    }
}