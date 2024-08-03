namespace Payments.Service.Models;

public class Payment
{
    public Payment()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
}