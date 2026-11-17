using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_task : BaseEntity
{
    public Guid id { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string application_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    public string title { get; set; } = string.Empty;

    [Column(TypeName = "varchar(max)")]
    public string? description { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string request_by { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime request_date { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? due_date { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? completed_date { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string status_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string task_type_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string task_priority_id { get; set; } = string.Empty;

    public Guid? import_batch_id { get; set; }
}
