using System.Reflection;

namespace Tangsem.Tooling.CodeGen.DtoCopy;

public static class TypeUtils
{
  /// <summary>
  /// Test if it Is ComplexType.
  /// </summary>
  public static bool IsComplexType(this Type type)
  {
    Type[] otherPrimitiveTypes =
      [typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid)];

    if (type.IsPrimitive
        || type.IsEnum
        || otherPrimitiveTypes.Contains(type))
    {
      return false;
    }

    var underlyingType = Nullable.GetUnderlyingType(type);
    if (underlyingType != null)
    {
      return IsComplexType(underlyingType);
    }

    return true;
  }

  public static bool IsComplexTypeProperty(this PropertyInfo propertyInfo)
    => propertyInfo.PropertyType.IsComplexType();
  
  /// <summary>
  /// Get ImplicitComplexTypes.
  /// </summary>
  public static HashSet<Type> GetComplexTypes_from_Properties(this ICollection<Type> types)
  {
    var otherTypes = types
      .SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Select(p => p.PropertyType))
      .Where(IsComplexType)
      .Except(types)
      .ToHashSet();

    if (otherTypes.Any())
    {
      var moreTypes = GetComplexTypes_from_Properties(otherTypes.Concat(types).ToList());

      return otherTypes.Union(moreTypes).ToHashSet();
    }
        
    return otherTypes;
  }
}