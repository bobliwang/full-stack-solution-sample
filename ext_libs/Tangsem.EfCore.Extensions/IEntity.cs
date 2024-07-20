namespace Tangsem.EfCore.Extensions;

public interface IEntity : IRowGuid
{
  int RowId { get; set; }

  bool? Active { get; set; }

  DateTimeOffset CreatedAt { get; set; }

  DateTimeOffset? ModifiedAt { get; set; }
}