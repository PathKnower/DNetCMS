//using DNetCMS.Interfaces;
//using DNetCMS.Models.DataContract;
//using System.Threading.Tasks;

//namespace DNetCMS.Repositories
//{
//    public class UnitOfWork : IUnitOfWork
//    {
//        ApplicationContext _context;

//        IRepository<News> _news;

//        IRepository<FileModel> _files;

//        public IRepository<News> News 
//        {
//            get { return _news ?? (_news = new Repository<News>(_context)); }
//        }

//        public IRepository<FileModel> Files 
//        {
//            get { return _files ?? (_files = new Repository<FileModel>(_context)); }
//        }

//        public UnitOfWork(ApplicationContext context)
//        {
//            _context = context;
//        }

//        private bool disposed = false;

//        public int SaveChanges()
//        {
//            return _context.SaveChanges();
//        }

//        public Task<int> SaveChangesAsync()
//        {
//            return _context.SaveChangesAsync();
//        }

//        public Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
//        {
//            return _context.SaveChangesAsync(cancellationToken);
//        }

//        public virtual void Dispose(bool disposing)
//        {
//            if (!this.disposed)
//            {
//                if (disposing)
//                {
//                    _context.Dispose();
//                }
//                this.disposed = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            //GC.SuppressFinalize(this);
//        }
//    }
//}