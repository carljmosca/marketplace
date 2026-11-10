using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Domain;

namespace ShopApp.Features.UC020_CancelOrder;

public sealed class UC020_Handler(IDbContextFactory<AppDbContext> dbFactory, TimeProvider clock)
{
    public async Task<List<UC020_OrderRow>> ListOrdersAsync(long customerId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // UC-020 BR-002: A customer only sees and cancels the orders they placed.
        return await db.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PlacedAt)
            .Select(o => new UC020_OrderRow(o.Id, o.Status.ToString(), o.PlacedAt))
            .ToListAsync(ct);
    }

    public async Task<UC020_Result> CancelAsync(UC020_Command command, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // UC-020 BR-002: A customer only sees and cancels the orders they placed.
        var order = await db.Orders.SingleAsync(
            o => o.Id == command.OrderId && o.CustomerId == command.CustomerId, ct);

        if (order.Status == OrderStatus.Shipped)
            return new UC020_Result(false, "Shipped orders cannot be cancelled");

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = clock.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return new UC020_Result(true, "Order cancelled");
    }
}
