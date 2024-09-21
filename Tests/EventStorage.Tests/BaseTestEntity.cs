using EventStorage.Configurations;

namespace EventStorage.Tests;

public abstract class BaseTestEntity
{ 
    public static InboxAndOutboxSettings InboxAndOutboxSettings { get; set; } 
}