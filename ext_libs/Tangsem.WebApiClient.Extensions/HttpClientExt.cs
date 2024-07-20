
namespace Tangsem.WebApiClient.Extensions;

using System.Net.Http.Json;
using System.Text.Json;

public static class HttpClientExt
{
  /// <summary>
  /// GetAsJson_Async
  /// </summary>
  public static async Task<T> GetAsJson_Async<T>(this HttpClient hc, string url, JsonSerializerOptions options = null)
  {
    var response = await hc.GetAsync(url);
    
    if (!response.IsSuccessStatusCode)
    {
      await response.Throw_On_ErrorResponse_Async();
    }
    
    return await response.ProcessSuccessResponse_Async<T>(options);
  }
  
  /// <summary>
  /// PostAsJson_Async
  /// </summary>
  public static async Task<T> PostAsJson_Async<T>(this HttpClient hc, string url, object reqBody = null, JsonSerializerOptions options = null)
  {
    var response = await hc.PostAsJsonAsync(url, reqBody, options);
    
    if (!response.IsSuccessStatusCode)
    {
      await response.Throw_On_ErrorResponse_Async();
    }
    
    return await response.ProcessSuccessResponse_Async<T>(options);
  }

  /// <summary>
  /// ProcessSuccessResponse_Async
  /// </summary>
  private static async Task<T> ProcessSuccessResponse_Async<T>(this HttpResponseMessage response, JsonSerializerOptions options = null)
  {
    string responseText = null;
    try
    {
      responseText = await response.Content.ReadAsStringAsync();
      options ??= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

      return JsonSerializer.Deserialize<T>(responseText, options);
    }
    catch (Exception ex)
    {
      throw new ApiClientException(message: "Reading response and deserialiation failed.", innerException: ex, statusCode: (int)response.StatusCode, responseText);
    }
  }

  /// <summary>
  /// Handle error response.
  /// </summary>
  private static async Task Throw_On_ErrorResponse_Async(this HttpResponseMessage response)
  {
    try
    {
      var respText = await response.Content.ReadAsStringAsync();

      throw new ApiClientException($"Request failed with status code {response.StatusCode}.",
        (int)response.StatusCode, respText);
    }
    catch (Exception ex)
    {
      throw new ApiClientException(
        $"Request failed with status code {response.StatusCode}. Worse: Unable to read response either: {ex}",
        (int)response.StatusCode, null);
    }
  }
}