using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tangsem.EfCore.Extensions;

[Index(nameof(RowGuid))]
public class EntityBase : IEntity
{
  [Key]
  [Column("row_guid")]
  public virtual Guid RowGuid { get; set; } = Guid.NewGuid();

  /// <summary>
  /// Gets or sets the server side row id.
  /// </summary>
  [Column("row_id")]
  public virtual int RowId { get; set; }

  [Column("active")]
  public bool? Active { get; set; } = true;

  [Column("created_at")]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("modified_at")]
  public DateTimeOffset? ModifiedAt { get; set; }
}