using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class rf_plan_feature : BaseEntity
{
    public long id { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string plan_id { get; set; } = string.Empty;

    [StringLength(200)]
    public string feature_text { get; set; } = string.Empty;

    public int order_seq { get; set; }
}
