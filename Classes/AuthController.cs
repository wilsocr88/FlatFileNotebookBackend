using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FlatFileStorage.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authSvc = authService;

        [HttpGet]
        [Authorize(Policy = "Bearer")]
        public bool CheckAuth()
        {
            return _authSvc.CheckAuth(Request.HttpContext.User.Identity.Name);
        }

        [HttpPost]
        public LoginResponse Login(LoginRequest req)
        {
            string uri = Request.Scheme + @"://" + Request.Host;
            return _authSvc.Login(req, uri);
        }

        [HttpPost]
        public bool CreateUser(LoginRequest req)
        {
            return _authSvc.CreateUser(req);
        }
    }
}
