namespace FlatFileStorage
{
    public class ReorderRequest
    {
        public string File { get; set; }
        public int CurrentPos { get; set; }
        public int NewPos { get; set; }
        public ReorderRequest()
        {
        }
    }
}
