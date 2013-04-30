using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebFrontend.ActionFilters;

namespace WebFrontend.Controllers
{
    [IncludeLayoutData]
    public class WaitStaffController : Controller
    {
        public ActionResult Todo(string id)
        {
            ViewBag.Waiter = id;
            return View(Domain.OpenTabQueries.TodoListForWaiter(id));
        }
    }
}
