using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence;

public static class MoneyPropertyBuilderExtensions
{
    public const int MoneyPrecision = 18;

    public const int MoneyScale = 2;

    public static PropertyBuilder<decimal> AsMoney(this PropertyBuilder<decimal> builder) =>
        builder.HasPrecision(MoneyPrecision, MoneyScale);

    public static PropertyBuilder<decimal?> AsMoney(this PropertyBuilder<decimal?> builder) =>
        builder.HasPrecision(MoneyPrecision, MoneyScale);
}
