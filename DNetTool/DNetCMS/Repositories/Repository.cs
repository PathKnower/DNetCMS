using System;
using DNetCMS.Interfaces;
using DNetCMS.Models.DataContract;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;


namespace DNetCMS.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        DbContext _context;
        DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public void Create(TEntity item)
        {
            _dbSet.Add(item);
        }
        public TEntity CreateR(TEntity item)
        {
            var res = _dbSet.Add(item);
            return res.Entity;
        }

        public TEntity Get(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] navigationProperties)
        {
            TEntity res;

            IQueryable<TEntity> dbQuery = _dbSet;

            //Apply eager loading
            foreach (Expression<Func<TEntity, object>> navigationProperty in navigationProperties)
                dbQuery = dbQuery.Include<TEntity, object>(navigationProperty);

            res = dbQuery
                .AsNoTracking()
                .FirstOrDefault(predicate);

            return res;
        }

        public IList<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigationProperties)
        {
            List<TEntity> list;

            IQueryable<TEntity> dbQuery = _dbSet;

            //Apply eager loading
            foreach (Expression<Func<TEntity, object>> navigationProperty in navigationProperties)
                dbQuery = dbQuery.Include<TEntity, object>(navigationProperty);

            list = dbQuery
                .AsNoTracking()
                .ToList<TEntity>();

            return list;
        }

        public TEntity GetById(object id)
        {
            return _dbSet.Find(id);
        }
        public Task<TEntity> GetByIdAsync(object id)
        {
            return _dbSet.FindAsync(id);
        }

        public IList<TEntity> GetMany(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] navigationProperties)
        {
            List<TEntity> list;

            IQueryable<TEntity> dbQuery = _dbSet;

            //Apply eager loading
            foreach (Expression<Func<TEntity, object>> navigationProperty in navigationProperties)
                dbQuery = dbQuery.Include<TEntity, object>(navigationProperty);

            list = dbQuery
                .AsNoTracking()
                .Where(predicate)
                .ToList<TEntity>();

            return list;
        }

        public void Remove(TEntity item)
        {
            _dbSet.Remove(item);
        }

        public void RemoveById(object id)
        {
            var c = _dbSet.Find(id);
            if (c == null)
            {
                return;
            }
            _dbSet.Remove(c);
        }

        public void Update(TEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }
    }
}