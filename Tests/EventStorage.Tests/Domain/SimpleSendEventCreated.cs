using EventStorage.Models;
using EventStorage.Outbox.Models;

namespace EventStorage.Tests.Domain;

public class SimpleSendEventCreated: ISendEvent, IHasHeaders
{ 
    public Guid Id { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Dictionary<string, string> Headers { get; set; }
}