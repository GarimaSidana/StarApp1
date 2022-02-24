using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using System.Security.Claims;

namespace StarApp1.Models
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<AuthenticationHandler> _logger;
        public AuthenticationHandler(IConfiguration configuration,
        IHttpContextAccessor httpContext, ILogger<AuthenticationHandler> logger)
        {
            _configuration = configuration;
            _httpContext = httpContext;
            _logger = logger;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }

        public async Task Login(RoleModel model, string redirectUrl, bool rememberMe)
        {
            try
            {
                //List<Claim> claims = new List<Claim> {
                //new Claim(ClaimTypes.Name, model.Login) };

                //claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                //ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


                
            }



            catch (Exception ex)
            {
                _logger.LogError("AuthenticationHandler Error", ex);
            }


        }
    }
}
    

