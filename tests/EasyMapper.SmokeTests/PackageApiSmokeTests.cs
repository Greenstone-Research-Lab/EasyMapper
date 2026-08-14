using Xunit;

namespace EasyMapper.SmokeTests;

public sealed class PackageApiSmokeTests
{
    [Fact]
    public void PublicApi_MapsARealisticDtoWithoutConfiguration()
    {
        Order source = new()
        {
            Id = 7,
            Customer = "Greenstone",
            Total = 149.90m,
        };

        OrderDto destination = source.MapTo<OrderDto>();

        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Customer, destination.Customer);
        Assert.Equal(source.Total, destination.Total);
    }

    private sealed class Order
    {
        public int Id { get; set; }

        public string Customer { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }

    private sealed class OrderDto
    {
        public int Id { get; set; }

        public string Customer { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}
