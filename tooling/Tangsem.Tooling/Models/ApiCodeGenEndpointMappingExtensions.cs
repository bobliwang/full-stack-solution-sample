using Tangsem.Tooling.CodeGen.ApiClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Tangsem.Tooling.Models;

public static class ApiCodeGenEndpointMappingExtensions
{
  public static RouteHandlerBuilder MapCodeGenEndpoints(this IEndpointRouteBuilder app, string routePattern = "_api/api_client_code_gen")
  {
    return app.MapGet(routePattern, () =>
      {
        var allEntries = app.DataSources.Select(x => x.GenerateApiCode()).ToArray();

        return allEntries.Select(x =>
        {
          return x.EndpointsMetadata.Select(x => new
          {
            ResponseType = x.ResponseTypeFullName,
            x.UrlPattern,
            x.DisplayName,
            x.MethodName,
            x.AllParameters,
            x.HttpMethod,
            x.GeneratedCode
          });
        });
      })
      .WithName("GenerateApiClientCode")
      .IgnoreCodeGen("This is Code Gen endpoint")
      .Produces<List<ApiEndpointMetaModel>>();
  }
}