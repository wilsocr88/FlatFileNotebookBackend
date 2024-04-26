namespace FlatFileStorage
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public LoginResponse()
        {
            Token = "";
            Success = false;
        }
    }
}