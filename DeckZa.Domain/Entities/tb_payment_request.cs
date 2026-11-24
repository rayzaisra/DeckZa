using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_payment_request : BaseEntity
{
    public long id { get; set; }

    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    public string request_no { get; set; } = string.Empty;

    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string plan_id { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal amount { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    public string currency { get; set; } = "IDR";

    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    public string payment_method { get; set; } = "TRANSFER";

    [StringLength(100)]
    public string? transfer_reference { get; set; }

    [StringLength(255)]
    public string proof_file_name { get; set; } = string.Empty;

    [StringLength(500)]
    public string proof_stored_path { get; set; } = string.Empty;

    [StringLength(100)]
    public string proof_content_type { get; set; } = string.Empty;

    public long proof_size_bytes { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string status { get; set; } = "PENDING";

    [StringLength(100)]
    public string? reviewed_by { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? reviewed_date { get; set; }

    [StringLength(500)]
    public string? review_note { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? activated_from { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? activated_to { get; set; }
}
