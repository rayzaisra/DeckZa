using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class rf_status : BaseEntity
{
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    public string status_id { get; set; } = string.Empty;

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    public string description { get; set; } = string.Empty;

    public int order_seq { get; set; }
}
