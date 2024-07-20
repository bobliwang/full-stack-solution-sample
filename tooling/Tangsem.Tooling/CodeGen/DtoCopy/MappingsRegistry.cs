namespace Tangsem.Tooling.CodeGen.DtoCopy;

public class MappingsRegistry
{
  public readonly HashSet<(Type source, Type dest)> Mappings = new();
  
  public void Add(Type source, Type dest)
  {
    this.Mappings.Add((source, dest));
  }
  
  public void Add((Type source, Type dest) pair)
  {
    this.Mappings.Add(pair);
  }

  public void Add<TSource, TDest>()
  {
    this.Mappings.Add((typeof(TSource), typeof(TDest)));
  }

  public void DiscoverImplicitTypeMappings()
  {
    // self mappings
    this.Mappings.ToList().ForEach(pair =>
    {
      var srcProps = new[] { pair.source }.GetComplexTypes_from_Properties();
      var dstProps = new[] { pair.dest }.GetComplexTypes_from_Properties();
      
      srcProps.ToList().ForEach(x => this.Mappings.Add((x, x)));
      dstProps.ToList().ForEach(x => this.Mappings.Add((x, x)));
    });

    this.Mappings.ToList().ForEach(pair =>
    {
      // pair.source
      DtoCopyCodeGen.GetPropertyPairs(pair.source, pair.dest).ForEach(propPair =>
      {
        if (propPair.srcProp.IsComplexTypeProperty() && propPair.destProp.IsComplexTypeProperty())
        {
          this.Mappings.Add((propPair.srcProp.PropertyType, propPair.destProp.PropertyType));
          this.Mappings.Add((propPair.destProp.PropertyType, propPair.srcProp.PropertyType));
        }
      });
    });
    
    foreach (var (src, dest) in this.Mappings.ToList())
    {
      this.Mappings.Add((src, src));
      this.Mappings.Add((dest, dest));
      this.Mappings.Add((dest, src));
    }
  }
}