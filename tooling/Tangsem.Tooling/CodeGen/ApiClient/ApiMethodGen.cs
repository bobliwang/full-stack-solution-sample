﻿using System.Web;
using Tangsem.Tooling.Models;
using Microsoft.AspNetCore.Routing;

namespace Tangsem.Tooling.CodeGen.ApiClient;

using MergeCodeGenOpts = (
  string namespaceName,
  string clientClassName,
  string[] usingNamespaces,
  Func<ApiEndpointMetaModel, bool> predicate
);

public static class ApiMethodGen
{
  public static string GenerateApiClientCode(this IEndpointRouteBuilder endpointRouteBuilder, MergeCodeGenOpts opts)
  {
    ApiCodeGenResponse[] allEntries = endpointRouteBuilder.DataSources
                                        .Select(endpointDataSource => endpointDataSource.GenerateApiCode())
                                        .ToArray();

    var clientCode = allEntries.MergeClientCode(opts);

    return clientCode;
  }
  
  public static ApiCodeGenResponse GenerateApiCode(this EndpointDataSource endpointDataSource)
  {
    var metaModels = endpointDataSource.BuildApiEndpointMetaModels().ToList();
    foreach (var metaModel in metaModels)
    {
      metaModel.GeneratedCode = metaModel.GenerateCode_for_SingleEndpoint();
    }
    
    return new ApiCodeGenResponse
    {
      EndpointsMetadata = metaModels,
    };
  }
  
  /// <summary>
  /// ToCsharpTypeName
  /// </summary>
  public static string MergeClientCode(this ApiCodeGenResponse[] allEntries, MergeCodeGenOpts opts)
  {
    var entries = allEntries.SelectMany(entry =>
    {
      return entry.EndpointsMetadata
        .Where(x => opts.predicate(x))
        .Select(x => new
          {
            ResponseType = x.ResponseTypeFullName,
            x.UrlPattern,
            x.DisplayName,
            x.MethodName,
            x.AllParameters,
            x.HttpMethod,
            x.GeneratedCode
          });
    }).ToList();
    
    string[] usingStmts = [
      "System.Net.Http",
      typeof(HttpUtility).Namespace,
      ..opts.usingNamespaces
    ];
    
    var clientCode = 
$$"""
namespace {{opts.namespaceName}};

{{string.Join(Environment.NewLine, usingStmts.Select(x => $"using {x};"))}}

/// <summary>
/// Generated part of {{opts.clientClassName}} class.
/// </summary>
public partial class {{opts.clientClassName}}
{
  private readonly HttpClient _hc;

  public ImageSearchApiClient(HttpClient hc)
  {
    this._hc = hc;
  }

  {{string.Join(Environment.NewLine + Environment.NewLine, entries.Select(x => x.GeneratedCode)).TrimStart()}}
}
""";
    return clientCode;
  }
  
  /// <summary>
  /// ToCsharpTypeName
  /// </summary>
  public static string ToCsharpTypeName(this Type type)
  {
    Dictionary<Type, string> primitiveMappings = new()
    {
      { typeof(int), "int" },
      { typeof(long), "long" },
      { typeof(double), "double" },
      { typeof(float), "float" },
      { typeof(bool), "bool" },
      { typeof(Guid), "Guid" },
      { typeof(DateTime), "DateTime" },

      { typeof(string), "string" },
      { typeof(byte[]), "byte[]" },
    };

    if (primitiveMappings.TryGetValue(type, out var fullClassName))
    {
      return fullClassName;
    }
    
    var underlyingType = Nullable.GetUnderlyingType(type);
    if (underlyingType != null)
    {
      return underlyingType.ToCsharpTypeName() + "?";
    }

    if (type.IsGenericType)
    {
      var argsExpr = $"{string.Join(", ", type.GetGenericArguments().Select(x => x.ToCsharpTypeName()))}";
      
      return $"{type.Name.Split('`')[0]}<{argsExpr}>";
    }
    
    return type.FullName;
  }

  /// <summary>
  /// GenerateCode for a single endpoint.
  /// </summary>
  private static string GenerateCode_for_SingleEndpoint(this ApiEndpointMetaModel metaModel)
  {
    var args = metaModel.AllParameters.Select(p => $"{p.ParameterType.ToCsharpTypeName()} {p.Name}").ToList();

    var returnType = metaModel.ResponseType.ToCsharpTypeName();

    var bodyArgNames = metaModel.AllParameters.Where(x => x.Kind == ApiParamKind.RequestBody).Select(x => x.Name);
    var bodyArgs     = string.Join(", ", bodyArgNames);

    if (bodyArgs.Any())
    {
      bodyArgs = ", " + bodyArgs;
    }
    
    var returnStmt = metaModel.HttpMethod switch
    {
      "POST" => $"this._hc.PostAsJson_Async<{returnType}>(url{bodyArgs})",
      "PUT" => $"this._hc.PostAsJson_Async<{returnType}>(url{bodyArgs})",
      
      "GET"  => $"this._hc.GetAsJson_Async<{returnType}>(url)",
    };

    var qryStrParams = metaModel.AllParameters.Where(x => x.Kind == ApiParamKind.QueryString).ToList();
    
    var qsStmt = qryStrParams.Any()
      ?
$$"""
    // build a query string with ts
    var dic = new Dictionary<string, object>
    {
      {{string.Join(Environment.NewLine, qryStrParams.Select(x => $"[\"{x.Name}\"] = {x.Name},"))}}
    };

    var qs = "?" + string.Join("&", dic.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value.ToString())}"));
    url += qs;
""" 
      : string.Empty;
    
    return
$$"""
  /// <summary>
  /// {{metaModel.MethodName}} Async.
  /// </summary>
  public async Task<{{metaModel.ResponseType.ToCsharpTypeName()}}> {{metaModel.MethodName}}_Async({{string.Join(", ", args)}})
  {
    var url = $"{{metaModel.UrlPattern}}";
    {{qsStmt}}
    
    return await {{returnStmt}};
  }
""";
  }
}