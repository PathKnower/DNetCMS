//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.Threading;
//using DNetCMS.Models.DataContract;

//namespace DNetCMS.Interfaces
//{
//    public interface IUnitOfWork : IDisposable
//    {
//        IRepository<News> News { get; }

//        IRepository<FileModel> Files { get; } 

//        #region Methods
//        int SaveChanges();
//        Task<int> SaveChangesAsync();
//        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
//        #endregion
//    }
//}