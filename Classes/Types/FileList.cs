using System;
using System.Collections.Generic;

namespace FlatFileStorage
{
    public class FileList
    {
        public List<string> files { get; set; }
        public FileList()
        {
            files = new List<string>();
        }
    }
}
