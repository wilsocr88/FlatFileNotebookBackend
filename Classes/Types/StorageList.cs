using System.Collections.Generic;

namespace FlatFileStorage
{
    public class StorageList
    {
        public List<Item> Items { get; set; }
        public StorageList()
        {
            Items = [];
        }
    }
}
