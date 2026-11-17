using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class um_group_app : BaseEntity
{
    [StringLength(50)]
    public string group_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? group_name { get; set; }
}
