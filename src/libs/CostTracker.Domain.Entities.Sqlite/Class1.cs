using CostTracker.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CostTracker.Domain.Entities.Sqlite;

public class CostTrackerDbContextSqlite(string connString) : CostTrackerDbContextBase(connString)
{
  public static CostTrackerDbContextSqlite FromMemory()
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
