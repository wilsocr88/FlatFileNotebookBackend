using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlatFileStorage.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authSvc;

        public AuthController(AuthService authService)
        {
            _authSvc = authService;
        }

        [HttpPost]
        public LoginResponse Login(LoginRequest req)
        {
            string uri = this.Request.Scheme + @"://" + this.Request.Host;
            return _authSvc.Login(req, uri);
        }
    }
}
