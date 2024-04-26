using System.Collections.Generic;

namespace FlatFileStorage
{
    public class UserList
    {
        public List<User> Users { get; set; }
        public UserList()
        {
            Users = [];
        }
    }
}
