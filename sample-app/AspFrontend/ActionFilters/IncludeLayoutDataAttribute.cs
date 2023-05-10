using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspFrontend.ActionFilters
{
    public class IncludeLayoutDataAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is ViewResult)
            {
                var bag = (filterContext.Result as ViewResult)!.ViewData;
                bag["WaitStaff"] = StaticData.WaitStaff;
                bag["ActiveTables"] = Domain.OpenTabQueries!.ActiveTableNumbers();
            }
        }
    }
}