using System;
using System.Collections.Generic;

namespace FlatFileStorage
{
    public class UserList
    {
        public List<User> users { get; set; }
        public UserList()
        {
            users = new List<User>();
        }
    }
}
