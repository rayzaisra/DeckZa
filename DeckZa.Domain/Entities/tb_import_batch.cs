using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_import_batch : BaseEntity
{
    public Guid import_batch_id { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    public string file_name { get; set; } = string.Empty;

    [Column(TypeName = "varchar(64)")]
    [StringLength(64)]
    public string file_hash { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string import_by { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime import_date { get; set; }

    public int total_rows { get; set; }

    public int total_success { get; set; }

    public int total_rejected { get; set; }

    [Column(TypeName = "varchar(1000)")]
    [StringLength(1000)]
    public string? reject_summary { get; set; }
}
