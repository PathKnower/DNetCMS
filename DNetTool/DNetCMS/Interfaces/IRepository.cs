using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;

namespace DNetCMS.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Create(TEntity item);
        TEntity CreateR(TEntity item);
        Task<TEntity> GetByIdAsync(object id);
        TEntity GetById(object id);
        TEntity Get(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] navigationProperties);
        IList<TEntity> GetMany(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] navigationProperties);
        IList<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigationProperties);
        void RemoveById(object id);
        void Remove(TEntity item);
        void Update(TEntity item);
    }
}