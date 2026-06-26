using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Patsanstha.BuildingBlocks.Infrastructure.Persistence;

public static class ConcurrencyTokenConfiguration
{
    public static PropertyBuilder<byte[]> ConfigureRowVersion(this PropertyBuilder<byte[]> propertyBuilder) =>
        propertyBuilder
            .IsConcurrencyToken()
            .HasColumnType("bytea")
            .ValueGeneratedNever();
}
