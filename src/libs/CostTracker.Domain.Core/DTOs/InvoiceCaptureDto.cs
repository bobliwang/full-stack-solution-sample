namespace CostTracker.Domain.Core.DTOs;

public class InvoiceCaptureDto
{
  public string InvoiceNumber { get; set; }

  public decimal TotalAmount { get; set; }

  public string RawText { get; set; }
}