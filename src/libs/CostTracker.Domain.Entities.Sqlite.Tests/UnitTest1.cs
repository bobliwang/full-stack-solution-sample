using Microsoft.EntityFrameworkCore;

namespace CostTracker.Domain.Entities.Sqlite.Tests;

[TestClass]
public class UnitTest1
{
  [TestMethod]
  public async Task TestMethod1()
  {
    var db = CostTrackerDbContextSqlite.FromMemory();
    await db.Database.EnsureCreatedAsync();

    var invoicesCount = await db.InvoiceCaptures.CountAsync();
    Assert.AreEqual(0, invoicesCount);

    db.InvoiceCaptures.Add(new InvoiceCapture
    {
      InvoiceNumber = "Test 123",
      TotalAmount = 20.5m
    });

    await db.SaveChangesAsync();

    db.ChangeTracker.Clear();

    var invoiceNumber = "Test 123";
    var invoice = await db.InvoiceCaptures.Where(x => x.InvoiceNumber == invoiceNumber).FirstOrDefaultAsync();

    Assert.IsNotNull(invoice);
    Assert.AreEqual(invoice.TotalAmount, 20.5m);
  }
}