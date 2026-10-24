using Services.Interfaces;

namespace MangaProjectApi.CustomMiddleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtUtils)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            // DEBUG: Ver qué cabecera está llegando realmente al servidor
            Console.WriteLine($"DEBUG Middleware: Authorization Header recibido = '{authHeader}'");

            if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                Console.WriteLine($"DEBUG Middleware: Token extraído = '{token}'");

                var userInfo = jwtUtils.Validate(token);
                if (userInfo != null)
                {
                    context.Items["Client"] = userInfo;
                    Console.WriteLine($"DEBUG Middleware: Usuario '{userInfo.Username}' asignado con éxito a HttpContext.Items[\"Client\"]");
                }
                else
                {
                    Console.WriteLine("DEBUG Middleware: La validación del token falló (Validate devolvió null).");
                }
            }
            else
            {
                Console.WriteLine("DEBUG Middleware: No se encontró un header Authorization válido o no empieza con 'Bearer '.");
            }

            await _next(context);
        }
    }
}
