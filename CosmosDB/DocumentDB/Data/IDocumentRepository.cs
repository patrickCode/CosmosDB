using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentDB.Data
{
    public interface IDocumentRepository<T> where T : class
    {
        Task Delete(string id);
        Task<Tuple<IEnumerable<T>, string>> Get(Func<T, bool> predicate);
        Task<T> Get(string id);
        Task<string> Insert(T item);
        Task Update(string id, T item);
    }
}