using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DNetCMS.Interfaces;

namespace DNetCMS.Components
{
    public class DynamNews : ViewComponent
    {
        IUnitOfWork Database;

        public DynamNews(IUnitOfWork uow)
        {
            Database = uow;
            //Database = new UnitOfWork();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            


            return View();
        }
    }
}