using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_export_job : BaseEntity
{
    [Key]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string job_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string? file_name { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? export_type { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? expires_at { get; set; }
}
