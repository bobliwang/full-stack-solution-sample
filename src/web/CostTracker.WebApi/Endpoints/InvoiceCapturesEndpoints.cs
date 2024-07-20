using CostTracker.Domain.Core.DTOs;
using CostTracker.Domain.Entities.Sqlite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostTracker.WebApi.Endpoints;

public class InvoiceCapturesEndpoints
{
  public void RegisterEndpoints(IEndpointRouteBuilder endpoints)
  {
    endpoints.MapGet("/invoice-captures", async ([FromServices] CostTrackerDbContextSqlite dbCtx) =>
    {
      var invoiceCaptures = await dbCtx.InvoiceCaptures
        .Select(x => new InvoiceCaptureDto
        {
          TotalAmount = x.TotalAmount,
          InvoiceNumber = x.InvoiceNumber,
          RawText = x.RawText
        })
        .ToListAsync();

      return invoiceCaptures;
    })
    .WithName("GetInvoiceCaptures")
    .WithOpenApi();

    endpoints.MapPost("/add-invoice-captures", async ([FromBody] InvoiceCaptureDto dto, [FromServices] CostTrackerDbContextSqlite dbCtx) =>
      {
        var entity = new InvoiceCapture
        {
          InvoiceNumber = dto.InvoiceNumber,
          RawText = dto.RawText,
          TotalAmount = dto.TotalAmount,
        };

        dbCtx.InvoiceCaptures.Add(entity);
        await dbCtx.SaveChangesAsync();

        return ResponsePayload.Success(entity.RowGuid);
      })
      .WithName("AddInvoiceCapture")
      .WithOpenApi();;
  }
}