namespace Tangsem.WebApiClient.Extensions;
using System.Diagnostics;
using System.Text.Json;

public class ApiClientException : Exception
{
  public ApiClientException(string message, int statusCode, string responseText = null) : base(message)
  {
    this.StatusCode   = statusCode;
    this.ResponseText = responseText;
  }

  public ApiClientException(string message, Exception innerException, int statusCode, string responseText = null) : base(message, innerException)
  {
    this.StatusCode   = statusCode;
    this.ResponseText = responseText;
  }

  public int StatusCode { get; set; }
  
  public string ResponseText { get; set; }

  
  public ApiErrorPayload TryGetApiErrorPayload()
  {
    try
    {
      return JsonSerializer.Deserialize<ApiErrorPayload>(this.ResponseText);
    }
    catch (Exception ex)
    {
      Trace.TraceWarning(ex.ToString());
      
      return null;
    }
  }
}
