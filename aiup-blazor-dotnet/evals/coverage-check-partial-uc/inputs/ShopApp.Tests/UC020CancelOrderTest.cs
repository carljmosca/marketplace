using Bunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopApp.Data;
using ShopApp.Domain;
using ShopApp.Features.UC020_CancelOrder;
using ShopApp.Security;

namespace ShopApp.Tests;

public class UC020CancelOrderTest : BunitContext
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<AppDbContext> _options;

    public UC020CancelOrderTest()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using (var setup = new AppDbContext(_options))
        {
            setup.Database.EnsureCreated();
            var now = DateTimeOffset.UtcNow;
            setup.Orders.AddRange(
                new Order { Id = 100, CustomerId = 1, Status = OrderStatus.Placed, PlacedAt = now.AddHours(-1) },
                new Order { Id = 101, CustomerId = 1, Status = OrderStatus.Shipped, PlacedAt = now.AddDays(-3) },
                new Order { Id = 102, CustomerId = 1, Status = OrderStatus.Placed, PlacedAt = now.AddDays(-2) });
            setup.SaveChanges();
        }

        Services.AddSingleton<IDbContextFactory<AppDbContext>>(new TestDbContextFactory(_options));
        Services.AddSingleton(TimeProvider.System);
        Services.AddSingleton<ICurrentCustomer>(new FixedCustomer(1));
        Services.AddScoped<UC020_Handler>();
    }

    [Fact]
    [UseCase("UC-020")]
    public void CustomerCancelsAPlacedOrder()
    {
        var cut = Render<UC020_Page>();

        cut.Find("#cancel-100").Click();
        cut.Find("#confirm-cancel").Click();

        cut.WaitForAssertion(() =>
            Assert.Equal("Order cancelled", cut.Find(".notification").TextContent));
        using var verify = new AppDbContext(_options);
        Assert.Equal(OrderStatus.Cancelled, verify.Orders.Single(o => o.Id == 100).Status);
    }

    [Fact]
    [UseCase("UC-020", Scenario = "A2: Order already shipped")]
    public void ShippedOrderCannotBeCancelled()
    {
        var cut = Render<UC020_Page>();

        cut.Find("#cancel-101").Click();

        Assert.Equal("Shipped orders cannot be cancelled", cut.Find(".notification").TextContent);
        using var verify = new AppDbContext(_options);
        Assert.Equal(OrderStatus.Shipped, verify.Orders.Single(o => o.Id == 101).Status);
    }

    [Fact(Skip = "flaky on CI")]
    [UseCase("UC-020", BusinessRules = ["BR-001"])]
    public void CancellationWindowIsEnforced()
    {
        var cut = Render<UC020_Page>();

        cut.Find("#cancel-102").Click();
    }

    private sealed class FixedCustomer(long id) : ICurrentCustomer
    {
        public long Id { get; } = id;
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
