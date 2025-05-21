using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GCTL.Core;
using GCTL.Core.DataTables;
using GCTL.Core.ViewModels.Accounts;
using GCTL.UI.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol.Core.Types;
using System.Linq.Expressions;

namespace GCTL.UI.Core.Controllers
{
    public abstract class BaseController : Controller
    {
        public UserInfoViewModel LoginInfo => GetCurrentSession();
       
        private UserInfoViewModel GetCurrentSession()
        {
            return HttpContext.Session.Get<UserInfoViewModel>(nameof(ApplicationConstants.LoginSessionKey));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (LoginInfo == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Accounts",
                    action = "Login"
                }));
            }
            base.OnActionExecuting(context);
        }

        protected OkObjectResult DataTablesResult<T>(PagedList<T> paginatedItems)
        {
            return Ok(new
            {
                recordsTotal = paginatedItems.TotalCount,
                recordsFiltered = paginatedItems.TotalCount,
                data = paginatedItems
            });
        }

       


    }
}
