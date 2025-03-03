
using System.Text.Json;
using CostTracker.Domain.Core.DTOs;
using CostTracker.WebApiClient;

namespace CostTracker.ApiClient.Tests;

[TestClass]
public class CostTrackerApiClientTests
{
  [TestMethod]
  public async Task GetWeatherForecast_Test()
  {
    var client = CreateApiClient();
    var result = await client.GetInvoiceCaptures_Async();

    Console.WriteLine(JsonSerializer.Serialize(result));

    Assert.AreNotEqual(result.Count, 0);
  }

  [TestMethod]
  public async Task AddWeatherForecast_Test()
  {
    var client = CreateApiClient();

    var result = await client.AddInvoiceCapture_Async(new InvoiceCaptureDto
    {
      InvoiceNumber = DateTime.UtcNow.Ticks.ToString(),
      RawText = "my raw text",
      TotalAmount = 22222m
    });

    Console.WriteLine("New invoice created: " + result.Data);

    Assert.AreNotEqual(result.Data, Guid.Empty);
  }

  private static CostTrackerApiClient CreateApiClient()
  {
    var client =  new CostTrackerApiClient(new HttpClient
    {
      BaseAddress = new Uri("http://localhost:5290/")
    });
    return client;
  }
}

