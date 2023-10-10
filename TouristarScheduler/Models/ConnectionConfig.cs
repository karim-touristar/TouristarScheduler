namespace TouristarScheduler.Models;

public class ConnectionConfig
{
    public string DbConnection { get; set; }
    public string RabbitConnection { get; set; }
    public string RabbitUser { get; set; }
    public string RabbitPassword { get; set; }
    public string RabbitPort { get; set; }
}