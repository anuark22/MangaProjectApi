using Models;

namespace Services.Interfaces
{
    public interface IJwtService
    {
        public string Generate(UsersDTO user, double hours = 8);
        public Users Authenticate(Users user);
        public UsersDTO? Validate(string token);
        public bool ValidateUser(int userId, UsersDTO requestingUser);
        public bool ValidateRecaptcha(string response);
    }
}
