using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUsersService
    {
        Task<Response<Users>> InsertRegister(Models.Users model);
        Response<Users> GetUsers(int? userId);
        Response<UsersDTO> Login(Models.UsersDTO model, bool recaptchaCheck = true, double hours = 8);
        Task<Response<UsersDTO>> GoogleAuthAsync(string idToken);
        Response<UserProfileDTO> GetUserProfile(int userId);
        Task<Response<Users>> UpdateProfile(UpdateProfileDTO model, int userId);
        Task<Response<Users>> ChangePassword(ChangePasswordDTO model, int userId);
        Task<Response<Users>> ForgotPassword(ForgotPasswordRequestDTO model);
        Response<Users> ValidateRecoveryToken(ValidateTokenDTO model);
        Task<Response<Users>> ResetPassword(ResetPasswordDTO model);
        Task<Response<DBClass>> UpdateUserRole(UpdateUserRoleDTO model, int adminUserId);
    }
}
