using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class rf_task_type : BaseEntity
{
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string task_type_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    public string description { get; set; } = string.Empty;

    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    public string owner_user_id { get; set; } = string.Empty;

    public int order_seq { get; set; }
}
