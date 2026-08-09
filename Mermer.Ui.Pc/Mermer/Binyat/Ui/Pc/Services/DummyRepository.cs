using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Data.Models; // <--- ДОБАВЛЕНО

//Временная заглушка(потом удалить!)

namespace Mermer.Ui.Pc.Services
{
    public class DummyRepository<T> : IRepository<T>, IReadOnlyRepository<T> where T : class, IModel
    {
        public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult(Enumerable.Empty<T>());
        public Task<T> GetAsync(string id) => Task.FromResult<T>(null);
        public Task<IEnumerable<T>> GetAsync(string[] ids) => Task.FromResult(Enumerable.Empty<T>());
        public Task<IEnumerable<T>> GetAsync(params Expression<Func<T, bool>>[] predicates) => Task.FromResult(Enumerable.Empty<T>());
        public Task<int> CountAsync(params Expression<Func<T, bool>>[] predicates) => Task.FromResult(0);
        public Task SaveAsync(T entity) => Task.CompletedTask;
        public Task CreateAsync(T entity) => Task.CompletedTask;
        public Task UpdateAsync(T entity) => Task.CompletedTask;
        public Task DeleteAsync(string id) => Task.CompletedTask;
    }
}