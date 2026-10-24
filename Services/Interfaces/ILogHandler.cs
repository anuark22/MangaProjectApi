using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Routing;

namespace Services.Interfaces
{
    public interface ILogHandler
    {
        void WriteRequest(RouteData routeData, StringBuilder parameterList, string eventName);
        void WriteError(Exception ex, [CallerMemberName] string memberName = "");
        void WriteTrace(string message, [CallerMemberName] string memberName = "");
        void WriteTraceFacebook(string message, string body = "", string extra = "");
        Task WriteErrorAsync(Exception ex, [CallerMemberName] string memberName = "");
        Task WriteTraceAsync(string message, string body = "");
    }
}
