
namespace Models
{
    public class Insertions<Q>
    {
        public int InsertedId { get; set; }
        public Q? Item { get; set; }
        public long? Timeleft { get; set; }
    }
    // Response Insertion DTO
    public class Response<T>
    {
        public int Status { get; set; }
        public bool Error { get; set; }
        public string? ErrorDescription { get; set; }
        public int? DataCount { get; set; }
        public ICollection<T>? Items { get; set; }
        public Response()
        {
            Items = new List<T>();
        }
        public void ParseResponse(bool returnEmptyList = false, string? Err = null)
        {
           if(!returnEmptyList && Items != null && !Items.Any())
            {
                Status = 404; // Not Found
                Error = true;
                if (Err != null) 
                {
                    ErrorDescription = Err;
                }
                else
                {
                     ErrorDescription = "Not Found";
                }
                return;
            }
            Status = 200; // Successful
            Error = false;
        }

        public void ParseExecution(int status, string? errorDescription)
        {
            Status = status;
            Error = errorDescription != null && errorDescription != "";
            ErrorDescription = errorDescription;
          
        }

    }   
    public class Response <T,Q> : Response<T>
    {
        public void ParseExecution(int status, string? errorDescription, int? insertedId = null, dynamic? item = null,long? timeLeft = null)
        {
            Status = status;
            Error = errorDescription != null && errorDescription != "";
            ErrorDescription = errorDescription;
            if(insertedId != null && insertedId != 0)
            {
                List<Insertions<Q>> list = new();
                Insertions<Q> i = new();
                i.InsertedId = insertedId.Value;
                i.Timeleft = timeLeft;
                i.Item = item;
                list.Add(i);
                Items = (ICollection<T>?) list;
            }
        }
    }
}
