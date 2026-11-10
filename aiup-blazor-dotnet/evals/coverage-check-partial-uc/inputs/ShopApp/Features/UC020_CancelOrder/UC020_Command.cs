namespace ShopApp.Features.UC020_CancelOrder;

public sealed record UC020_Command(long CustomerId, long OrderId);

public sealed record UC020_OrderRow(long Id, string Status, DateTimeOffset PlacedAt);

public sealed record UC020_Result(bool Cancelled, string Message);
