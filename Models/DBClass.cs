namespace Models
{
    public class DBClass
    {
        public int Status { get; set; }
        public string? ErrorDescription { get; set; }
    }

    public class DBInsertions : DBClass
    {
        public int InsertedId { get; set; }
    }

    public class DBClassName: DBClass
    {
        public string Name { get; set; }
    }
    public class DBCLassPassword : DBClass
    {
        public string Password { get; set; }
    }
    public class DBClassBPFiles : DBClass
    {
        public int BPId { get; set; }
        public string FilePath { get; set; }
    }
}
