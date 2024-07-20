using CostTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CostTracker.Domain.Repositories;

public class CostTrackerDbContextBase(string connStr) : DbContext
{
  /// <summary>
  /// The InvoiceCaptures.
  /// </summary>
  public DbSet<InvoiceCapture> InvoiceCaptures { get; set; }
}
