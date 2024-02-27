using System;
using System.Collections.Generic;

namespace FlatFileStorage
{
    public class StorageList
    {
        public List<Item> items { get; set; }
        public StorageList()
        {
            items = new List<Item>();
        }
    }
}
