using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_task_history : BaseEntity
{
    public int id { get; set; }

    public Guid task_id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? from_status_id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? to_status_id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? from_priority_id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? to_priority_id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string change_type { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? field_name { get; set; }

    [Column(TypeName = "varchar(1000)")]
    [StringLength(1000)]
    public string? old_value { get; set; }

    [Column(TypeName = "varchar(1000)")]
    [StringLength(1000)]
    public string? new_value { get; set; }

    [Column(TypeName = "varchar(1000)")]
    [StringLength(1000)]
    public string? notes { get; set; }

    public int order_seq { get; set; }
}
