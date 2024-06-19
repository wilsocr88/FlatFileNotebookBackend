using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace FlatFileStorage;

public class AuthService
{
    private readonly SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Ji9qNQ94nHYfoOekjhyhsO8376hGF6bh"));
    private readonly AppSettings _config;
    private readonly UserList _users;
    private readonly string FilePath;
    public AuthService()
    {
        // Get config
        var configJson = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
        _config = JsonSerializer.Deserialize<AppSettings>(configJson);

        // Working directory
        FilePath = Path.Combine(Directory.GetCurrentDirectory(), _config.WorkingDirectory);

        _users = ReadUsers();
        if (_users.Users.Count == 0)
        {
            string salt = GenerateSalt();
            WriteUser(new User()
            {
                Email = _config.DefaultEmail,
                Salt = salt,
                HashedPassword = HashPassword(_config.DefaultPassword, salt)
            });
        }
    }

    public string GenerateSalt()
    {
        byte[] saltBytes = new byte[128 / 8];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
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
        LoginResponse res = new();
        if (CheckPassword(req.User, req.Pass))
        {
            res.Success = true;
            res.Token = GenerateTokenForUser(req.User, authURI);
        }
        return res;
    }
    public bool CheckPassword(string email, string pass)
    {
        UserList users = ReadUsers();
        User user = users.Users.Find(u => u.Email == email);
        if (object.Equals(user, default(User)))
        {
            return false;
        }
        string hashedPasswordAttempt = HashPassword(pass, user.Salt);
        if (hashedPasswordAttempt != user.HashedPassword)
        {
            return false;
        }
        return true;
    }
    public string GenerateTokenForUser(string user, string authURI)
    {
        string token = "";
        SigningCredentials signingCreds = new(key,
            SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
        ClaimsIdentity claimsID = new(new List<Claim>()
            {
                new(ClaimTypes.Name, user)
            }, "Custom");
        SecurityTokenDescriptor securityTokenDescriptor = new()
        {
            Expires = DateTime.Now.AddYears(1),
            Subject = claimsID,
            SigningCredentials = signingCreds,
            Audience = authURI,
            Issuer = "MisterDizzy"
        };
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
        token = tokenHandler.WriteToken(plainToken);
        return token;
    }
    public bool WriteUser(User user)
    {
        // Get file (or new empty set)
        UserList userList = ReadUsers();
        // Add item
        userList.Users.Add(user);
        try
        {
            // Send to file
            string json = JsonSerializer.Serialize(userList);
            using StreamWriter outputFile = new(Path.Combine(FilePath, _config.UsersTable));
            outputFile.Write(json);
            outputFile.Close();
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
        UserList response = new();
        // If we just created the file new, return an empty set
        if (CreateUserFile())
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
        User checkUser = users.Users.Find(u => u.Email == req.User);
        if (!Equals(checkUser, default(User)))
        {
            return false;
        }
        return WriteUser(new User()
        {
            Email = req.User,
            Salt = salt,
            HashedPassword = HashPassword(req.Pass, salt)
        });
    }

    public bool CheckAuth(string user)
    {
        LoginResponse response = new();
        UserList users = ReadUsers();
        User checkUser = users.Users.Find(u => u.Email == user);
        if (Equals(checkUser, default(User)))
        {
            return false;
        }
        return true;
    }
}