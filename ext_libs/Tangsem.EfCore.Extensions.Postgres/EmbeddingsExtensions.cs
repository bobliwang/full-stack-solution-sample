namespace Tangsem.EfCore.Extensions.Postgres;

using Pgvector;

public static class EmbeddingsExtensions
{
  public static Vector AsVector(this float[] values) => new (values);
}
