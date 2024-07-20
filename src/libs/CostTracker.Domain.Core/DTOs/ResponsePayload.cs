namespace CostTracker.Domain.Core.DTOs;

public class ResponsePayload
{
  public bool IsSuccess { get; set; }

  public string ErrorMessage { get; set; }

  public object Data { get; set; }

  public static ResponsePayload<T> Success<T>(T data)
  {
    return new ResponsePayload<T>
    {
      IsSuccess = true,
      Data      = data
    };
  }

  public static ResponsePayload<T> Failure<T>(T data, string errorMessage)
  {
    return new ResponsePayload<T>
    {
      IsSuccess = false,
      Data      = data,
      ErrorMessage = errorMessage
    };
  }
}


public class ResponsePayload<T> : ResponsePayload
{
  public bool IsSuccess { get; set; }

  public new T Data { get; set; }
}