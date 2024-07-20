using System.Reflection;

namespace Tangsem.Tooling.CodeGen.DtoCopy;

public static class DtoCopyCodeGen
{
  /// <summary>
  /// GenCopyMethod
  /// </summary>
  public static string GenCopyMethod(this Type srcType, Type destType, MappingsRegistry mappingReg)
  {
    var props = GetPropertyPairs(srcType, destType, mappingReg);

    var maxLen = props.Select(x => x.destProp.Name.Length).Max();

    var code =
      $$"""
           
          /// <summary>
          /// Copy values to {{destType.Name}} from src.
          /// </summary>
          public static void CopyTo_{{destType.Name}}(this {{srcType.FullName}} src, {{destType.FullName}} dest)
          {
             {{string.Join(Environment.NewLine, props.Select((pair, idx) =>
             {
               if (pair.srcProp.IsComplexTypeProperty())
               {
                 return
                   $$"""
                         dest.{{pair.destProp.Name.PadRight(maxLen, ' ')}} = src.{{pair.srcProp.Name}}?.CopyOf_{{pair.destProp.PropertyType.Name}}();
                     """;
               }
               
               if (Nullable.GetUnderlyingType(pair.srcProp.PropertyType) != null && Nullable.GetUnderlyingType(pair.destProp.PropertyType) == null)
               {
                 return
                   $$"""
                         if (src.{{pair.srcProp.Name}} != null)
                         {
                           dest.{{pair.destProp.Name.PadRight(maxLen, ' ')}} = src.{{pair.srcProp.Name}};
                         }
                     """;
               }

               return
                 $"""
                      dest.{pair.destProp.Name.PadRight(maxLen, ' ')} = src.{pair.srcProp.Name};
                  """;
             }))}}
          }
          
        """;
    
    code += $$"""
              
                /// <summary>
                /// Make a copy of {{destType.Name}} from src.
                /// </summary>              
                public static {{destType.FullName}} CopyOf_{{destType.Name}}(this {{srcType.FullName}} src)
                {
                  var dest = new {{destType.FullName}}();
                  
                  src.CopyTo_{{destType.Name}}(dest);
                  
                  return dest;
                }
              """;
    
    return code;
  }
  
  /// <summary>
  /// Gets the PropertyPairs
  /// </summary>
  public static List<(PropertyInfo srcProp, PropertyInfo destProp)> GetPropertyPairs(Type srcType, Type destType, MappingsRegistry mappingReg = null)
  {
    // find all readable properties from srcType
    var srcProps = srcType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
      .Where(p => p.CanRead).ToList();

    // find all writable properties from destType
    var destProps = destType
      .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
      .Where(p => p.CanWrite).ToList();

    // join them by property name and type (tolerate nullable>
    var propPairs = srcProps.Join(destProps, x => x.Name, y => y.Name, (srcProp, destProp) => (srcProp, destProp))
      .Where(x => x.srcProp.PropertyType == x.destProp.PropertyType
                  || Nullable.GetUnderlyingType(x.srcProp.PropertyType) == x.destProp.PropertyType
                  || x.srcProp.PropertyType == Nullable.GetUnderlyingType(x.destProp.PropertyType)
                  || mappingReg?.Mappings.Contains((x.srcProp.PropertyType, x.destProp.PropertyType)) == true)
      .ToList();

    return propPairs;
  }
}