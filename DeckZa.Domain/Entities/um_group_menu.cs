using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class um_group_menu : BaseEntity
{
    public long id { get; set; }

    [StringLength(20)]
    public string? group_id { get; set; }

    public long? menu_id { get; set; }
}
