using System.Collections.Generic;

namespace FlatFileStorage
{
    public class FileList
    {
        public List<string> Files { get; set; }
        public FileList()
        {
            Files = [];
        }
    }
}
