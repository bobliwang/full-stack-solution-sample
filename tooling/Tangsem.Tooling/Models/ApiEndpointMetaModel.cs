using System.Text.Json.Serialization;

namespace Tangsem.Tooling.Models;

public class ApiCodeGenResponse
{
  public List<ApiEndpointMetaModel> EndpointsMetadata { get; set; }
}

public class ApiEndpointMetaModel
{
  public string DisplayName { get; set; }
  
  public string MethodName { get; set; }
  
  public string HttpMethod { get; set; }
  
  public string UrlPattern { get; set; }
  
  public Type RequestType { get; set; }

  public Type ResponseType  { get; set; }
  
  public string RequestTypeFullName => this.RequestType?.FullName;

  public string ResponseTypeFullName => this.ResponseType?.FullName;
  
  public List<ParameterMetadataModel> RouteParams { get; set; }
  
  public List<ParameterMetadataModel> AllParameters { get; set; }
  
  public List<ParameterMetadataModel> BodyParameters { get; set; }
  
  public string GeneratedCode { get; set; }
}

public enum ApiParamKind
{
  /// <summary>
  /// In query string.
  /// </summary>
  QueryString,
  
  /// <summary>
  /// Part of the route path.
  /// </summary>
  RoutePath,
  
  /// <summary>
  /// Sends in the header.
  /// </summary>
  HttpHeader,
  
  /// <summary>
  /// The request body.
  /// </summary>
  RequestBody,
}

public class ParameterMetadataModel
{
  public string Name { get; set; }
  
  [JsonIgnore]
  public Type ParameterType { get; set; }

  public string TypeFullName
    => this.ParameterType.FullName;
  
  public ApiParamKind Kind { get; set; }

  public string KindName => this.Kind.ToString();
}