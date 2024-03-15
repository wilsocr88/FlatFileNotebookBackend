using System;
using System.Collections.Generic;

namespace FlatFileStorage
{
    public class ReorderRequest
    {
        public string file { get; set; }
        public int currentPos { get; set; }
        public int newPos { get; set; }
        public ReorderRequest()
        {
        }
    }
}
