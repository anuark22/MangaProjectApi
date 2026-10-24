using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;
using Services.Interfaces;
using Models;
using System.Globalization;
namespace Services
{
    public class UsersService : IUsersService
    {
        private readonly ICommonService _commonService;
        private readonly IConfiguration _config;
        private readonly IJwtService _jwtService;

        public UsersService(ICommonService commonService, IJwtService jwtService, IConfiguration config)
        {
            _commonService = commonService;
            _jwtService = jwtService;
            _config = config;

        }
        public Response<UsersDTO> Login(Models.UsersDTO model, bool recaptchaCheck = true, double hours = 8)
        {
            Response<UsersDTO> response = new();
            //if (recaptchaCheck)
            //    if (!_jwtService.ValidateRecaptcha(model.Token))
            //    {
            //        response.ParseExecution(409, "Recaptcha Invalido");
            //        return response;
            //    }
            int Action = 1;
            object param = new { model.Email };
            var query = "SELECT * FROM Users WHERE Email = @Email";
            List<UsersDTO> u = _commonService.Query<UsersDTO>(query, param).ToList();
            if (u.Count == 0)
            {
                response.ParseExecution(404, "User Not Found");
                return response;
            }
            UsersDTO user = u.First();
            if (user.IsBlocked == 1)
            {
                response.ParseExecution(403, "User Banned");
                return response;
            }
            //if (user.Password == "")
            //{
            //    response.ParseExecution(403, "Correo registrado para inicio de sesion con Google/Facebook");
            //    return response;
            //}
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                response.ParseExecution(403, "Wrong Password");
                return response;
            }
            u.ForEach(us =>
            {
                us.Token = _jwtService.Generate(us);
                us.Password = null;
            });
            response.Items = u;
            response.ParseResponse();
            return response;
        }
        public async Task<Response<Users>> InsertRegister(Models.Users model)
        {
            Response<Users> response = new();
            //if (!_jwtService.ValidateRecaptcha(model.Token))
            //{
            //    response.ParseExecution(409, "Recaptcha Invalido");
            //    return response;
            //}
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            int Action = 2;
            object param = new { Action, model.Username, model.Password,model.AceptTerms,model.Email };
            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UsersProcedures", param, CommandType.StoredProcedure);
            response.ParseExecution(dbResponse.ToArray()[0].Status, dbResponse.ToArray()[0].ErrorDescription);
            return response;
        }
        public Response<Users> GetUsers(int? userId)
        {
            Response<Users> response = new();
            int Action = 1;
            object param = new { Action, userId };
            response.Items = _commonService.Query<Users>("UsersProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, x =>
            {
                x.Password = null;
            });
            return response;
        }
    }
}
