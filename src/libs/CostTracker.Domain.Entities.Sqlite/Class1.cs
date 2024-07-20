using CostTracker.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CostTracker.Domain.Entities.Sqlite;

public class CostTrackerDbContextBaseSqlite(string connString) : CostTrackerDbContextBase(connString)
{
  public static CostTrackerDbContextBaseSqlite FromMemory()
    => new (connString: "DataSource=file::memory:?cache=shared");

  /// <summary>
  /// OnConfiguring
  /// </summary>
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlite(connString);
    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
  }

  /// <summary>
  /// OnModelCreating
  /// </summary>
  protected override void OnModelCreating(ModelBuilder mb)
  {
    base.OnModelCreating(mb);
  }
}
