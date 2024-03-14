using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace FlatFileStorage
{
    public class AuthService
    {
        private SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Ji9qNQ94nHYfoOekjhyhsO8376hGF6bh"));
        private readonly AppSettings _config;
        private readonly UserList _users;
        private readonly FileService _fileSvc;
        private string FilePath;
        public AuthService(FileService fileService)
        {
            _fileSvc = fileService;

            // Get config
            var configJson = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
            _config = JsonSerializer.Deserialize<AppSettings>(configJson);

            // Working directory
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), _config.WorkingDirectory);

            _users = ReadUsers();
            if (_users.users.Count == 0)
            {
                string salt = GenerateSalt();
                WriteUser(new User()
                {
                    email = _config.DefaultEmail,
                    salt = salt,
                    hashedPassword = HashPassword(_config.DefaultPassword, salt)
                });
            }
        }

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }
        public string HashPassword(string pass, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: pass,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }

        public LoginResponse Login(LoginRequest req, string authURI)
        {
            LoginResponse res = new LoginResponse();
            if (CheckPassword(req.user, req.pass))
            {
                res.success = true;
                res.token = GenerateTokenForUser(req.user, authURI);
            }
            return res;
        }
        public bool CheckPassword(string email, string pass)
        {
            UserList users = ReadUsers();
            User user = users.users.Find(u => u.email == email);
            if (object.Equals(user, default(User)))
            {
                return false;
            }
            string hashedPasswordAttempt = HashPassword(pass, user.salt);
            if (hashedPasswordAttempt != user.hashedPassword)
            {
                return false;
            }
            return true;
        }
        public string GenerateTokenForUser(string user, string authURI)
        {
            string token = "";
            SigningCredentials signingCreds = new SigningCredentials(key,
                SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
            ClaimsIdentity claimsID = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user)
            }, "Custom");
            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.Now.AddYears(1),
                Subject = claimsID,
                SigningCredentials = signingCreds,
                Audience = authURI,
                Issuer = "MisterDizzy"
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            token = tokenHandler.WriteToken(plainToken);
            return token;
        }

        public bool WriteUser(User user)
        {
            // Get file (or new empty set)
            UserList userList = ReadUsers();
            // Add item
            userList.users.Add(user);
            try
            {
                // Send to file
                string json = JsonSerializer.Serialize(userList);
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(FilePath, _config.UsersTable)))
                {
                    outputFile.Write(json);
                    outputFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        /**
         * Get contents of file, or return a new empty set
         */
        public UserList ReadUsers()
        {
            UserList response = new UserList();
            if (CreateUserFile())
                // If we just created the file new, return an empty set
                return response;

            // File already existed, parse its contents
            string text = File.ReadAllText(Path.Combine(FilePath, _config.UsersTable));
            // If it was empty, return an empty set
            if (string.IsNullOrEmpty(text)) return response;
            response = JsonSerializer.Deserialize<UserList>(text);
            return response;
        }
        private bool CreateUserFile()
        {
            string path = Path.Combine(FilePath, _config.UsersTable);
            try
            {
                if (!File.Exists(path))
                {
                    var file = File.Create(path);
                    file.Close();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }
        public bool CreateUser(LoginRequest req)
        {
            string salt = GenerateSalt();
            UserList users = ReadUsers();
            User checkUser = users.users.Find(u => u.email == req.user);
            if (!object.Equals(checkUser, default(User)))
            {
                return false;
            }
            return WriteUser(new User()
            {
                email = req.user,
                salt = salt,
                hashedPassword = HashPassword(req.pass, salt)
            });
        }

        public bool CheckAuth(string user)
        {
            LoginResponse response = new LoginResponse();
            UserList users = ReadUsers();
            User checkUser = users.users.Find(u => u.email == user);
            if (object.Equals(checkUser, default(User)))
            {
                return false;
            }
            return true;
        }
    }
}