using Models;

namespace Services.Interfaces
{
    public interface IUsersService
    {
        Task<Response<Users>> InsertRegister(Models.Users model);
        Response<Users> GetUsers(int? userId);
        Response<UsersDTO> Login(Models.UsersDTO model, bool recaptchaCheck = true, double hours = 8);
    }
}
