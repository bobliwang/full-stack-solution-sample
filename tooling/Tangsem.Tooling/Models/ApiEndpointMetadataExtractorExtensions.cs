using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Tangsem.Tooling.Models;

public static class ApiEndpointMetadataExtractorExtensions
{
  /// <summary>
  /// BuildApiEndpointMetaModels
  /// </summary>
  public static ApiEndpointMetaModel[] BuildApiEndpointMetaModels(this EndpointDataSource endpointDataSource)
  {
    var names = new List<ApiEndpointMetaModel>();
    
    foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
    {
      if (endpoint.Metadata.Any(x => x is IgnoreCodeGenMetadata))
      {
        break;
      }
      
      var httpMethod   = endpoint.Metadata.OfType<HttpMethodMetadata>().SelectMany(x => x.HttpMethods).FirstOrDefault();
      var urlPattern   = endpoint.RoutePattern.RawText;
      var reqBodyType  = endpoint.Metadata.OfType<AcceptsMetadata>().Select(op => op.RequestType).FirstOrDefault();
      var producesTypeMeta = endpoint.Metadata
        .OfType<ProducesResponseTypeMetadata>()
        .LastOrDefault(x => x.StatusCode >= 200 && x.StatusCode < 300);

      var endpointNameMeta = endpoint.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault();

      var methodInfo = endpoint.Metadata
        .OfType<MethodInfo>()
        .FirstOrDefault();

      if (methodInfo == null)
      {
        return Array.Empty<ApiEndpointMetaModel>();
      }
      
      var parameters = methodInfo.GetParameters().Where(x => !x.GetCustomAttributes<FromServicesAttribute>().Any()).ToList();
      
      names.Add(new ApiEndpointMetaModel
      {
        DisplayName   = endpoint.DisplayName,
        HttpMethod    = httpMethod,
        UrlPattern    = urlPattern,
        RequestType   = reqBodyType,
        ResponseType  = producesTypeMeta?.Type,
        MethodName    = endpointNameMeta?.EndpointName ?? methodInfo.Name,
        AllParameters = parameters.Select(x => x.ToParameterMetadataModel()).ToList(),
      });
    }

    return names.ToArray();
  }

  /// <summary>
  /// ToParameterMetadataModel
  /// </summary>
  public static ParameterMetadataModel ToParameterMetadataModel(this ParameterInfo x)
  {
    return new ParameterMetadataModel
    {
      Name          = x.Name,
      ParameterType = x.ParameterType,
      Kind          = GetKind(x),
    };
  }

  /// <summary>
  /// GetKind
  /// </summary>
  private static ApiParamKind GetKind(ParameterInfo x)
  {
    if (x.GetCustomAttributes<FromBodyAttribute>().FirstOrDefault() != null)
    {
      return ApiParamKind.RequestBody;
    }
    
    if (x.GetCustomAttributes<FromQueryAttribute>().FirstOrDefault() != null)
    {
      return ApiParamKind.QueryString;
    }
    
    if (x.GetCustomAttributes<FromHeaderAttribute>().FirstOrDefault() != null)
    {
      return ApiParamKind.HttpHeader;
    }
    
    return ApiParamKind.RoutePath;
  }
}