using AspFrontend.ActionFilters;
using Microsoft.AspNetCore.Mvc;

namespace AspFrontend.Controllers
{
    [IncludeLayoutData]
    public class WaitStaffController : Controller
    {
        public ActionResult Todo(string id)
        {
            ViewData["Waiter"] = id;
            return View(Domain.OpenTabQueries!.TodoListForWaiter(id));
        }
    }
}
