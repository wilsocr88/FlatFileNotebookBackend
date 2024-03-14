using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

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
        [HttpPost]
        public bool CreateUser(LoginRequest req)
        {
            return _authSvc.CreateUser(req);
        }
        [HttpGet]
        [Authorize(Policy = "Bearer")]
        public bool CheckAuth()
        {
            return _authSvc.CheckAuth(Request.HttpContext.User.Identity.Name);
        }
    }
}
