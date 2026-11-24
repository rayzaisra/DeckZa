using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class tb_payment_invoice : BaseEntity
{
    public long id { get; set; }

    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    public string invoice_no { get; set; } = string.Empty;

    public long payment_request_id { get; set; }

    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string plan_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string plan_name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal amount { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    public string currency { get; set; } = "IDR";

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string billing_period { get; set; } = "ONE_TIME";

    [Column(TypeName = "datetime2")]
    public DateTime invoice_date { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? activated_from { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? activated_to { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string status { get; set; } = "ISSUED";

    [StringLength(255)]
    public string file_name { get; set; } = string.Empty;

    [StringLength(500)]
    public string stored_path { get; set; } = string.Empty;

    [StringLength(100)]
    public string content_type { get; set; } = "application/pdf";

    public long size_bytes { get; set; }
}
