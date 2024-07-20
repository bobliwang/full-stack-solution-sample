using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tangsem.EfCore.Extensions.Postgres;


public static class RowIdAsIdentityEntityExtensions
{
  public static void RowId_IdentityAlways_Npgsql<TEntity>(this ModelBuilder mb) where TEntity : EntityBase
  {
    mb.Entity<TEntity>()
      .Property(x => x.RowId)
      .UseIdentityAlwaysColumn();
  }

  public static EntityTypeBuilder<TEntity> RowId_IdentityAlways_Npgsql<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : EntityBase
  {
    entityTypeBuilder
      .Property(x => x.RowId)
      .UseIdentityAlwaysColumn();

    return entityTypeBuilder;
  }
}