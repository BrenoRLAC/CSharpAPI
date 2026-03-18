namespace API.Domain
{
    public class UserSignalR
    {
        public string ConnectionId { get; set; }
        public string CodUser { get; set; }
        public DateTime DateTime { get; set; }
        public string Application { get; set; }
        public string Environment { get; set; }
    }
}
