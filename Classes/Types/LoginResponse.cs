
namespace FlatFileStorage
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public LoginResponse()
        {
            success = false;
        }
    }
}