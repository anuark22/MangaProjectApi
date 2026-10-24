using Microsoft.Extensions.Hosting;
using Services.Interfaces;
using Services;
using System.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using MangaProjectApi.CustomMiddleware;
using Microsoft.OpenApi.Models;

namespace MangaProjectApi
{
    public class Startup
    {
        public IConfiguration _config { get; }
        private readonly IHostEnvironment _hostEnvironment;
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _config = configuration;
            _hostEnvironment = hostEnvironment;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            //if (_config["Production"]!.ToString() == "YES")
            //{
            //    services.AddHostedService<BackgroundJob>();
            //}
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILogHandler, LogHandlerService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IGenresService, GenresService>();
            services.AddScoped<IDemographicsService, DemographicsService>();
            services.AddScoped<IScansGroupsService, ScansGroupsService >();
            services.AddScoped<IMangasTypeService, MangasTypeService>();
            services.AddScoped<IMangasService, MangasService>();
            services.AddScoped<IMangaGenresService, MangaGenresService>();
            services.AddScoped<IMangaChaptersService,  MangaChaptersService>();
            // 
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MangaProjectApi", Version = "v1" });

                // Configuración para el botón Authorize con JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            CommonService rep = new(_config);
            rep.Query<dynamic>("Create_Master_Data", new object(), CommandType.StoredProcedure);

            services.AddCors(cors => cors.AddPolicy("AllowOrigins", options => options.AllowAnyOrigin().AllowAnyMethod()));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"].Trim(), // ← Usar Trim()
                    ValidAudience = _config["Jwt:Audience"].Trim(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
                };
            });

            services.AddMvc();
            services.AddControllers().AddNewtonsoftJson((options) =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }); ;
            services.AddRazorPages();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });
            app.UseStaticFiles(new StaticFileOptions()
            {

                FileProvider = new PhysicalFileProvider(
                            Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
                RequestPath = new PathString("/Files")
            });
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
            app.UseResponseCompression();
            app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async (context) =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                if (exception != null)
                {
                    var logHandler = context.RequestServices.GetRequiredService<ILogHandler>();
                    await logHandler.WriteErrorAsync(exception);
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        StatusCode = 500,
                        Message = "An unexpected error occurred"
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse)).ConfigureAwait(false);
                }
            }));
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
