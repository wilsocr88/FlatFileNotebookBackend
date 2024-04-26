namespace FlatFileStorage
{
    public class User
    {
        public string Email { get; set; }
        public string Salt { get; set; }
        public string HashedPassword { get; set; }
    }
}
