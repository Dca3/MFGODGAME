using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Domain.Entities;

public class ItemAffix
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    public double Value { get; set; }
    public bool IsPercent { get; set; }
    
    // Navigation property
    public Item Item { get; set; } = null!;
}
