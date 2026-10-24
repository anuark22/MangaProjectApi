using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using Services.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace Services
{
    public class LogHandlerService : ILogHandler
    {
        Logger ErrorLog, TraceLog;

        public LogHandlerService()
        {
            ErrorLog = LogManager.GetLogger("ErrorLog");
            TraceLog = LogManager.GetLogger("TraceLog");
        }

        public void WriteRequest(RouteData routeData, StringBuilder parameterList, string eventName)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];

            StringBuilder logTrace = new StringBuilder();
            logTrace.AppendLine('\n' + "===============================================Start==================================================");
            logTrace.AppendLine("Action - " + actionName + ", Controller - " + controllerName + ", Event - " + eventName);
            logTrace.AppendLine("Date and Time - " + DateTime.Now.ToString());
            logTrace.AppendLine(parameterList.ToString());
            logTrace.AppendLine("================================================End==================================================");
            TraceLog.Info(logTrace);
        }

        public void WriteError(Exception ex, [CallerMemberName] string memberName = "")
        {
            StackFrame objStackFrame = new StackFrame(1);
            StringBuilder errorTrace = new StringBuilder();
            errorTrace.AppendLine('\n' + "=============================================Start====================================================");
            errorTrace.AppendLine("MemberName - " + memberName);
            errorTrace.AppendLine("Action - " + objStackFrame.GetMethod().Name + ", Controller - " + objStackFrame.GetMethod().DeclaringType.Name);
            errorTrace.AppendLine("Date and Time - " + DateTime.Now.ToString());
            errorTrace.AppendLine("Exception - " + ex.Message.ToString());
            errorTrace.AppendLine("==============================================End====================================================");
            ErrorLog.Error(errorTrace);
        }

        public void WriteTrace(string message, string body = "")
        {
            StringBuilder logTrace = new StringBuilder();
            logTrace.AppendLine('\n' + "===============================================Start==================================================");
            logTrace.AppendLine("Date and Time - " + DateTime.Now.ToString());
            logTrace.AppendLine(message.ToString());
            logTrace.AppendLine(body.ToString());
            logTrace.AppendLine("================================================End==================================================");
            TraceLog.Info(logTrace);
        }

        public async Task WriteTraceAsync(string message, string body = "")
        {
            StringBuilder logTrace = new StringBuilder();
            logTrace.AppendLine(Environment.NewLine + "=============================================== Start ==================================================");
            logTrace.AppendLine("Date and Time - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            logTrace.AppendLine(message);
            if (!string.IsNullOrEmpty(body))
            {
                logTrace.AppendLine(body);
            }
            logTrace.AppendLine("=============================================== End ====================================================");

            // Crear un evento de log de NLog
            var logEvent = new LogEventInfo
            {
                Level = LogLevel.Info,
                Message = logTrace.ToString()
            };

            // Registra el evento de log de manera asíncrona
            await Task.Run(() => TraceLog.Log(logEvent));
        }

        public async Task WriteErrorAsync(Exception ex, [CallerMemberName] string memberName = "")
        {
            StackFrame objStackFrame = new StackFrame(1);
            StringBuilder errorTrace = new StringBuilder();
            errorTrace.AppendLine(Environment.NewLine + "============================================= Start ====================================================");
            errorTrace.AppendLine("MemberName - " + memberName);
            errorTrace.AppendLine("Action - " + objStackFrame.GetMethod().Name + ", Controller - " + objStackFrame.GetMethod().DeclaringType.Name);
            errorTrace.AppendLine("Date and Time - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            errorTrace.AppendLine("Exception - " + ex.Message);
            errorTrace.AppendLine("StackTrace - " + ex.StackTrace);
            errorTrace.AppendLine("============================================== End =====================================================");

            // Crear un evento de log de NLog
            var logEvent = new LogEventInfo
            {
                Level = LogLevel.Error,
                Message = errorTrace.ToString()
            };

            // Registra el evento de log de manera asíncrona
            await Task.Run(() => ErrorLog.Log(logEvent));
        }


        public void WriteTraceFacebook(string message, string body = "", string extra = "")
        {
            StringBuilder logTrace = new StringBuilder();
            logTrace.AppendLine('\n' + "===============================================Start==================================================");
            logTrace.AppendLine(message.ToString() + "|" + body.ToString() + "|" + extra.ToString());
            logTrace.AppendLine("================================================End==================================================");
            TraceLog.Info(logTrace);
        }
    }
}
