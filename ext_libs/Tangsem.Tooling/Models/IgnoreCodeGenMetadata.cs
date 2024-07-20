using Microsoft.AspNetCore.Builder;

namespace Tangsem.Tooling.Models;

public class IgnoreCodeGenMetadata
{
  public string Reason { get; set; }
}

public static class EndpointConventionBuilderExt
{
  public static T IgnoreCodeGen<T>(this T self, string reason = null) where T : IEndpointConventionBuilder
  {
    self.WithMetadata(new IgnoreCodeGenMetadata { Reason = reason });
    
    return self;
  }
}