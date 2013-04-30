using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebFrontend.ActionFilters;

namespace WebFrontend.Controllers
{
    [IncludeLayoutData]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
