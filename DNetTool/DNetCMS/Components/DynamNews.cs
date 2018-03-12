using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using DNetCMS.Models.DataContract;


namespace DNetCMS.Components
{
    public class DynamNews : ViewComponent
    {
        ApplicationContext Database;

        public DynamNews(ApplicationContext context)
        {
            Database = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(Database.News.ToArray());
        }
    }
}