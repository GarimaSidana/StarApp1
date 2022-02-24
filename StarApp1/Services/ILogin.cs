using StarApp1.Models;

namespace StarApp1.Services
{
    public interface ILogin
    {
        int CheckLogin(LoginViewModel paramLogin);
    }
}
