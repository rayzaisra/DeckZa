using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeckZa.Domain.Entities;

public partial class um_user_app : BaseEntity
{
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? full_name { get; set; }

    [StringLength(255)]
    public string? password { get; set; }

    [StringLength(100)]
    public string? email { get; set; }

    [StringLength(20)]
    public string? phone_number { get; set; }

    [StringLength(1)]
    public string? gender_id { get; set; }

    public DateTime? last_password_change_date { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? registration_source { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? plan_id { get; set; }

    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    public string? external_provider { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    public string? external_provider_id { get; set; }

    [StringLength(500)]
    public string? avatar_url { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? profile_synced_date { get; set; }
}
