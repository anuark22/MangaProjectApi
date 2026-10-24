using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class GenresService : IGenresService
    {
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public GenresService(ICommonService commonService, IWebHostEnvironment env, IConfiguration config)
        {
            _commonService = commonService;
            _env = env;
            _config = config;

        }

        // =========================================================================
        // ACTION 1: Obtener todos los géneros o filtrar (Con URL multimedia completa en Paralelo)
        // =========================================================================
        public Response<Genres> GetGenres(int? genreId = null, string? genreName = null)
        {
            Response<Genres> response = new();
            int action = 1;
            object param = new { Action = action, GenreId = genreId, GenreName = genreName };

            // 1. Obtenemos la lista desde la base de datos
            response.Items = _commonService.Query<Genres>("GenresProcedures", param, CommandType.StoredProcedure);

            // 2. Parseamos la estructura básica de la respuesta
            response.ParseResponse(true);

            // 3. Obtenemos la URL base desde el appsettings.json
            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            // 4. Procesamos los elementos en paralelo para concatenar la URL de la imagen
            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, genre =>
            {
                if (!string.IsNullOrEmpty(genre.GenreImgUrl))
                {
                    // Combinamos la URL base y el nombre de la imagen de forma segura
                    genre.GenreImgUrl = $"{baseUrl.TrimEnd('/')}/{genre.GenreImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        // =========================================================================
        // ACTION 2: Crear género (Procesa el archivo y luego guarda en BD)
        // =========================================================================
        public async Task<Response<Genres>> CreateGenre(GenreCreateDTO model)
        {
            Response<Genres> response = new();

            // 1. Validar extensión del archivo en el servicio
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
            var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                // Aprovechamos tu ResponseHandler para devolver un 400 Bad Request
                response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png y .svg");
                return response;
            }

            try
            {
                // 2. Definir y asegurar que exista la carpeta "Files"
                string folderPath = Path.Combine(_env.ContentRootPath, "Files");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 3. Generar el nombre único con un GUID
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string fullFilePath = Path.Combine(folderPath, uniqueFileName);

                // 4. Guardar físicamente el archivo en el disco
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                // 5. Preparar el envío a la base de datos mapeando el DTO a la Entidad
                int action = 2;
                object param = new
                {
                    Action = action,
                    model.GenreName,
                    model.GenreDescription,
                    GenreImgUrl = uniqueFileName // Mandamos el nombre del archivo recién guardado
                };

                // 6. Ejecutar el procedimiento de inserción
                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("GenresProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error guardando el archivo, devolvemos un 500 controlado
                response.ParseExecution(500, $"Error procesando el archivo en el servidor: {ex.Message}");
            }

            return response;
        }

        // =========================================================================
        // ACTION 3: Actualizar género (Maneja imagen opcional e ISNULL de la BD)
        // =========================================================================
        public async Task<Response<Genres>> UpdateGenre(GenreUpdateDTO model)
        {
            Response<Genres> response = new();
            string? uniqueFileName = null; // Por defecto es null

            // 1. Si el usuario subió una nueva imagen, la procesamos
            if (model.File != null && model.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
                var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.ParseExecution(400, "Solo se permiten imágenes con formato .jpg, .png y .svg");
                    return response;
                }

                try
                {
                    string folderPath = Path.Combine(_env.ContentRootPath, "Files");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    string fullFilePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    response.ParseExecution(500, $"Error procesando el nuevo archivo de imagen: {ex.Message}");
                    return response;
                }
            }

            // 2. Ejecutar la actualización en base de datos
            try
            {
                int action = 3;
                object param = new
                {
                    Action = action,
                    model.GenreId,
                    model.GenreName,
                    model.GenreDescription,
                    GenreImgUrl = uniqueFileName // Si es null, el SP mantendrá la imagen anterior
                };

                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("GenresProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error al actualizar en la base de datos: {ex.Message}");
            }

            return response;
        }

        // =========================================================================
        // ACTION 4: Borrado lógico (Status = 0)
        // =========================================================================
        public async Task<Response<Genres>> DeleteGenre(int genreId)
        {
            Response<Genres> response = new();
            int action = 4;

            object param = new
            {
                Action = action,
                GenreId = genreId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("GenresProcedures", param, CommandType.StoredProcedure);

            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }
    }
}