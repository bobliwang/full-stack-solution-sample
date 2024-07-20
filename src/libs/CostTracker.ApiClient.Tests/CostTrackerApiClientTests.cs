
namespace CostTracker.ApiClient.Tests;

[TestClass]
public class CostTrackerApiClientTests
{
  [TestMethod]
  public async Task GetWeatherForecast_Test()
  {
    var result = await new CostTrackerApiClient(new HttpClient
    {
      BaseAddress = new Uri("http://localhost:5290/")
    }).GetWeatherForecast_Async();

    Assert.AreNotEqual(result.Length, 0);
  }
}

