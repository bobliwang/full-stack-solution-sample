using System.ComponentModel.DataAnnotations.Schema;
using Tangsem.EfCore.Extensions;

namespace CostTracker.Domain.Entities;

public class InvoiceCapture : EntityBase
{
  [Column("invoice_number")]
  public string InvoiceNumber { get; set; }

  [Column("total_amount")]
  public decimal TotalAmount { get; set; }

  [Column("raw_text")]
  public string RawText { get; set; }
}
