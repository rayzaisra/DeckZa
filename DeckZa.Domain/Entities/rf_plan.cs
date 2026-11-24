using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class rf_plan : BaseEntity
{
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string id { get; set; } = string.Empty;

    [StringLength(100)]
    public string plan_name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal price_amount { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    public string price_currency { get; set; } = "IDR";

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string billing_period { get; set; } = "ONE_TIME";

    public int billing_interval_count { get; set; } = 1;

    public int? max_active_tasks { get; set; }

    public int? report_period_months { get; set; }

    public bool allows_calendar { get; set; }

    public bool allows_excel_import { get; set; }

    public bool allows_supervisor_mode { get; set; }

    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string? upgrade_group_id { get; set; }

    public int order_seq { get; set; }
}
