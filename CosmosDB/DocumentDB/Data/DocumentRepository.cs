using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;

namespace DocumentDB.Data
{
    public class DocumentRepository<T> : IDocumentRepository<T> where T : class
    {
        private readonly string _endpoint;
        private readonly string _authKey;
        private readonly string _databaseId;
        private readonly string _collectionId;

        private readonly DocumentClient _client;

        public DocumentRepository(string endpoint, string authKey, string databaseId, string collectionId)
        {
            _endpoint = endpoint;
            _authKey = authKey;
            _databaseId = databaseId;
            _collectionId = collectionId;
            _client = new DocumentClient(new Uri(_endpoint), _authKey);
            CreateDatabase().Wait();
            CreateCollection().Wait();
        }

        private async Task CreateDatabase()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database() { Id = _databaseId }, new RequestOptions() { ConsistencyLevel = ConsistencyLevel.Eventual });
                }
            }
        }

        private async Task CreateCollection()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        new DocumentCollection() { Id = _collectionId },
                        new RequestOptions() { OfferThroughput = 400 });
                }
            }
        }

        public async Task<T> Get(string id)
        {
            try
            {
                var document = await _client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), new RequestOptions());
                return document;
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        public async Task<Tuple<IEnumerable<T>, string>> Get(Func<T, bool> predicate)
        {
            IDocumentQuery<T> query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                new FeedOptions { MaxItemCount = -1 })
                .AsDocumentQuery();
                
            //.Where(predicate)
            //.AsQueryable()

            var query2 = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate);
                
                


            string continuationToken = null;
            List<T> results = new List<T>();
            if (query.HasMoreResults)
            {
                var feed = await query.ExecuteNextAsync<T>();
                continuationToken = feed.ResponseContinuation;
                results.AddRange(feed.AsEnumerable());
            }
            else
            {
                continuationToken = null;
            }

            return new Tuple<IEnumerable<T>, string>(results, continuationToken);
        }

        public async Task<string> Insert(T item)
        {
            var document = await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), item);
            return document.Resource.Id;
        }

        public async Task Update(string id, T item)
        {
            await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), item);
        }

        public async Task Delete(string id)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id));
        }
    }
}