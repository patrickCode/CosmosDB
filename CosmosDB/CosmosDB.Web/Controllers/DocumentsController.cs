using CosmosDB.Web.Models;
using DocumentDB.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDB.Web.Controllers
{   
    [Route("api/books")]
    public class BooksController: Controller
    {
        private readonly IDocumentRepository<Book> _repository;

        public BooksController(IDocumentRepository<Book> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<Tuple<IEnumerable<Book>, string>> Get()
        {
            return await _repository.Get(book => true);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<Book> Get(string id)
        {
            return await _repository.Get(id);
        }

        [HttpPost]
        public async Task<string> Create([FromBody]Book book)
        {
            return await _repository.Insert(book);
        }

        [HttpPut]
        public async Task Update([FromBody]Book book)
        {
            await _repository.Update(book.Id, book);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task Delete(string id)
        {
            await _repository.Delete(id);
        }
    }
}