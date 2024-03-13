using System;

namespace FlatFileStorage
{
    public class User
    {
        public string email { get; set; }
        public string salt { get; set; }
        public string hashedPassword { get; set; }
    }
}
