using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Text;
using Services;
using Services.Interfaces;

namespace YuplonWebApi.CustomMiddleware
{
    public class LogAttribute : ActionFilterAttribute
    {
        LogHandlerService LogHandlerService;

        public LogAttribute()
        {
            LogHandlerService = new LogHandlerService();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            StringBuilder parameterList = new StringBuilder();
            parameterList.Append("Parameters List - ");
            parameterList.Append(JsonConvert.SerializeObject(filterContext.ActionArguments).ToString());
            LogHandlerService.WriteRequest(filterContext.RouteData, parameterList, "OnActionExecuting");
        }
    }

}
