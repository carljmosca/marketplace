namespace ShopApp.Domain;

public class Order
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
}
